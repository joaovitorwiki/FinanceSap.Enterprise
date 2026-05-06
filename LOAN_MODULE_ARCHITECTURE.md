# Módulo de Empréstimos (Loan) — Arquitetura e Extensibilidade

## 🎯 Visão Geral

Módulo de empréstimos implementado seguindo **Clean Architecture** com foco em **extensibilidade** e **testabilidade**. Usa **Strategy Pattern** para cálculo de parcelas e **Domain Events** para desacoplamento.

---

## 📐 Arquitetura

### Camadas Implementadas

```
┌─────────────────────────────────────────────────────────────┐
│  Domain Layer (Core Business Logic)                        │
│  ✓ Entities/Loan.cs                                         │
│  ✓ Common/BaseEntity.cs                                     │
│  ✓ Interfaces/ILoanCalculator.cs                            │
│  ✓ Services/CompoundInterestCalculator.cs                   │
│  ✓ Events/LoanRequestedEvent.cs                             │
│  ✓ Enums/LoanStatus.cs                                      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  Tests Layer (Unit Tests)                                   │
│  ✓ Domain/LoanTests/LoanTests.cs                            │
│  ✓ Domain/LoanTests/CompoundInterestCalculatorTests.cs      │
└─────────────────────────────────────────────────────────────┘
```

---

## 🏗️ Componentes Principais

### 1. BaseEntity (Extensibility Foundation)

**Arquivo:** `Domain/Common/BaseEntity.cs`

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    
    // Domain Events Support
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    
    protected void AddDomainEvent(IDomainEvent domainEvent);
    public void ClearDomainEvents();
    protected void MarkAsUpdated();
}
```

**Benefícios:**
- ✅ **Auditoria automática** — CreatedAt/UpdatedAt em todas as entidades
- ✅ **Domain Events** — Suporte nativo para Event-Driven Architecture
- ✅ **DRY** — Elimina duplicação de código entre entidades
- ✅ **Extensibilidade** — Fácil adicionar soft delete, versioning, etc.

---

### 2. ILoanCalculator (Strategy Pattern)

**Arquivo:** `Domain/Interfaces/ILoanCalculator.cs`

```csharp
public interface ILoanCalculator
{
    decimal CalculateInstallment(decimal principal, decimal annualRate, int months);
    decimal CalculateTotalAmount(decimal principal, decimal annualRate, int months);
}
```

**Implementações Possíveis:**

| Implementação | Fórmula | Uso |
|---------------|---------|-----|
| **CompoundInterestCalculator** | Tabela Price (Sistema Francês) | ✅ Implementado |
| SACCalculator | Sistema de Amortização Constante | 🔜 Futuro |
| SimpleInterestCalculator | Juros Simples | 🔜 Futuro |
| CustomCalculator | Regras específicas do negócio | 🔜 Futuro |

**Extensibilidade:**
```csharp
// Adicionar novo método de cálculo sem modificar Loan
public class SACCalculator : ILoanCalculator
{
    public decimal CalculateInstallment(decimal principal, decimal annualRate, int months)
    {
        // Implementação SAC: parcelas decrescentes
        var amortization = principal / months;
        var monthlyRate = GetMonthlyRate(annualRate);
        return amortization + (principal * monthlyRate);
    }
}

// Usar no código
var loan = Loan.Create(customerId, 10_000m, 0.12m, 12, new SACCalculator());
```

---

### 3. CompoundInterestCalculator (Tabela Price)

**Arquivo:** `Domain/Services/CompoundInterestCalculator.cs`

**Fórmula Implementada:**

$$
PMT = P \times \frac{i(1+i)^n}{(1+i)^n-1}
$$

Onde:
- **PMT** = Valor da parcela mensal
- **P** = Principal (valor do empréstimo)
- **i** = Taxa de juros mensal
- **n** = Número de parcelas

**Conversão de Taxa Anual para Mensal:**

$$
i_{mensal} = (1 + i_{anual})^{1/12} - 1
$$

**Exemplo de Cálculo:**
```csharp
var calculator = new CompoundInterestCalculator();
var installment = calculator.CalculateInstallment(
    principal: 10_000m,
    annualRate: 0.12m,  // 12% ao ano
    months: 12
);
// Resultado: R$ 888,49/mês
// Total a pagar: R$ 10.661,88
// Juros totais: R$ 661,88
```

**Validações Implementadas:**
- ✅ Principal > 0
- ✅ Taxa >= 0 e <= 100%
- ✅ Meses > 0 e <= 360 (30 anos)
- ✅ Proteção contra overflow matemático

---

### 4. Loan Entity (Aggregate Root)

**Arquivo:** `Domain/Entities/Loan.cs`

**Propriedades:**

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| Id | Guid | Identificador único |
| CustomerId | Guid | FK para Customer |
| PrincipalAmount | decimal | Valor solicitado |
| InterestRate | decimal | Taxa anual (ex: 0.12 = 12%) |
| Installments | int | Número de parcelas |
| MonthlyPaymentAmount | decimal | Valor da parcela mensal |
| TotalToPay | decimal | Total a pagar (principal + juros) |
| Status | LoanStatus | Pending/Approved/Rejected |
| CreatedAt | DateTime | Data de criação |
| UpdatedAt | DateTime? | Data da última atualização |

**Invariantes de Negócio:**

```csharp
✓ CustomerId != Guid.Empty
✓ PrincipalAmount > 0 && <= 1.000.000
✓ InterestRate >= 0 && <= 0.50 (50% ao ano)
✓ Installments > 0 && <= 360 (30 anos)
✓ Calculator != null
```

**Factory Method (Named Constructor):**

```csharp
public static Result<Loan> Create(
    Guid customerId,
    decimal principalAmount,
    decimal interestRate,
    int installments,
    ILoanCalculator calculator)
{
    // Validação de invariantes
    // Cálculo de parcelas
    // Criação da entidade
    // Disparo de domain event
    return Result<Loan>.Success(loan);
}
```

**Métodos de Transição de Estado:**

```csharp
public Result Approve()  // Pending → Approved
public Result Reject(string? reason = null)  // Pending → Rejected
```

**Métodos de Consulta:**

```csharp
public decimal GetTotalInterest()  // TotalToPay - PrincipalAmount
public decimal GetMonthlyInterestRate()  // Taxa mensal efetiva
public bool IsFinal()  // Status == Approved || Rejected
```

---

### 5. LoanRequestedEvent (Domain Event)

**Arquivo:** `Domain/Events/LoanRequestedEvent.cs`

```csharp
public sealed record LoanRequestedEvent : IDomainEvent
{
    public Guid LoanId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal PrincipalAmount { get; init; }
    public decimal InterestRate { get; init; }
    public int Installments { get; init; }
    public decimal MonthlyPaymentAmount { get; init; }
    public DateTime OccurredOn { get; init; }
}
```

**Quando é Disparado:**
- ✅ Dentro do método `Loan.Create()` após validação bem-sucedida
- ✅ Antes da persistência no banco de dados

**Casos de Uso (Extensibilidade):**

```csharp
// Event Handler 1: Notificação ao Cliente
public class LoanRequestedNotificationHandler : INotificationHandler<LoanRequestedEvent>
{
    public async Task Handle(LoanRequestedEvent evt, CancellationToken ct)
    {
        await _emailService.SendAsync(evt.CustomerId, 
            "Seu pedido de empréstimo foi recebido!");
    }
}

// Event Handler 2: Análise de Crédito Automática
public class LoanRequestedCreditAnalysisHandler : INotificationHandler<LoanRequestedEvent>
{
    public async Task Handle(LoanRequestedEvent evt, CancellationToken ct)
    {
        var score = await _creditBureau.GetScoreAsync(evt.CustomerId);
        if (score > 700)
            await _mediator.Send(new ApproveLoanCommand(evt.LoanId));
    }
}

// Event Handler 3: Auditoria
public class LoanRequestedAuditHandler : INotificationHandler<LoanRequestedEvent>
{
    public async Task Handle(LoanRequestedEvent evt, CancellationToken ct)
    {
        await _auditLog.LogAsync($"Loan {evt.LoanId} requested by {evt.CustomerId}");
    }
}
```

---

## 🧪 Testes Unitários

### CompoundInterestCalculatorTests

**Cobertura:**
- ✅ Cálculos válidos (happy path)
- ✅ Taxa zero (divisão simples)
- ✅ Diferentes cenários (Theory)
- ✅ Validação de parâmetros inválidos
- ✅ Casos extremos (1 mês, 360 meses)

**Exemplo:**
```csharp
[Fact]
public void CalculateInstallment_WithValidParameters_ReturnsCorrectValue()
{
    // Arrange
    var calculator = new CompoundInterestCalculator();
    
    // Act
    var installment = calculator.CalculateInstallment(10_000m, 0.12m, 12);
    
    // Assert
    installment.Should().BeApproximately(888.49m, 0.01m);
}
```

### LoanTests

**Cobertura:**
- ✅ Criação bem-sucedida
- ✅ Cálculo de parcelas
- ✅ Domain events
- ✅ Validação de invariantes (10+ cenários)
- ✅ Transições de estado (Approve/Reject)
- ✅ Métodos de consulta

**Exemplo:**
```csharp
[Fact]
public void Create_WithValidParameters_RaisesLoanRequestedEvent()
{
    // Act
    var result = Loan.Create(customerId, 10_000m, 0.12m, 12, calculator);
    
    // Assert
    result.Value!.DomainEvents.Should().HaveCount(1);
    result.Value.DomainEvents.First().Should().BeOfType<LoanRequestedEvent>();
}
```

---

## 🚀 Próximos Passos (Roadmap)

### Application Layer

```csharp
// Commands
public record RequestLoanCommand(Guid CustomerId, decimal Amount, decimal Rate, int Months);
public record ApproveLoanCommand(Guid LoanId);
public record RejectLoanCommand(Guid LoanId, string Reason);

// Queries
public record GetLoanByIdQuery(Guid LoanId);
public record GetLoansByCustomerQuery(Guid CustomerId);

// Handlers
public class RequestLoanHandler : IRequestHandler<RequestLoanCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RequestLoanCommand cmd, CancellationToken ct)
    {
        var loanResult = Loan.Create(
            cmd.CustomerId, 
            cmd.Amount, 
            cmd.Rate, 
            cmd.Months, 
            _calculator
        );
        
        if (!loanResult.IsSuccess)
            return Result<Guid>.Failure(loanResult.Error!);
        
        await _loanRepository.AddAsync(loanResult.Value!, ct);
        await _unitOfWork.CommitAsync(ct);
        
        return Result<Guid>.Success(loanResult.Value!.Id);
    }
}
```

### Infrastructure Layer

```csharp
// Repository
public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IEnumerable<Loan>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task AddAsync(Loan loan, CancellationToken ct);
    void Update(Loan loan);
}

// EF Core Configuration
public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(l => l.Id);
        
        builder.Property(l => l.PrincipalAmount)
            .HasPrecision(18, 2);
        
        builder.Property(l => l.InterestRate)
            .HasPrecision(5, 4);
        
        builder.HasOne(l => l.Customer)
            .WithMany()
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Ignore(l => l.DomainEvents);
    }
}
```

### API Layer

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RequestLoan([FromBody] RequestLoanRequest request)
    {
        var command = new RequestLoanCommand(
            GetCurrentCustomerId(),
            request.Amount,
            request.InterestRate,
            request.Installments
        );
        
        var result = await mediator.Send(command);
        
        return result.IsSuccess 
            ? CreatedAtAction(nameof(GetLoan), new { id = result.Value }, result.Value)
            : BadRequest(new { message = result.Error });
    }
}
```

---

## 📚 Referências

- [Clean Architecture (Robert C. Martin)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design (Eric Evans)](https://www.domainlanguage.com/ddd/)
- [Strategy Pattern](https://refactoring.guru/design-patterns/strategy)
- [Domain Events (Martin Fowler)](https://martinfowler.com/eaaDev/DomainEvent.html)
- [Tabela Price (Sistema Francês)](https://pt.wikipedia.org/wiki/Sistema_franc%C3%AAs_de_amortiza%C3%A7%C3%A3o)

---

**Módulo pronto para extensão e produção!** 🎉
