````markdown
Estou construindo uma API para um servidor de jogo MMO usando o template “Clean Architecture” do JasonTaylorDev (https://github.com/jasontaylordev/CleanArchitecture).  
Esse template já expõe automaticamente os endpoints de Identity via:

```csharp
app.MapIdentityApi<ApplicationUser>();
````

Além da entidade `ApplicationUser` (camada de Identity), tenho outra entidade de domínio chamada `Account` com regras de negócio próprias (value objects, validações, exceções de domínio).
O objetivo é que **sempre que um `ApplicationUser` for criado** (p.ex. via endpoint de registro de usuário do Identity), seja **automaticamente criada** uma instância de `Account` correspondente — para que o cliente não precise chamar dois endpoints diferentes.

**Pergunto**:

1. Como posso “amarrar” o fluxo de criação do Identity (`ApplicationUser`) com a criação do meu aggregate `Account` dentro do padrão Clean Architecture, sem violar a separação de camadas?
2. Que padrões/granularidade de projeto (eventos de domínio, notificações, handlers, Mediator, decorators, *middleware*, etc.) recomendam para implementar essa “sincronização”?
3. Qual seria a estrutura de pastas, projetos e dependências entre camadas (API, Application, Domain, Infrastructure) para suportar este fluxo de forma coesa e escalável?
4. Exemplifique, de forma concisa, como ficariam:

   * O ponto de extensão no `Program.cs` (ou `WebApplicationBuilder`) para mapear Identity e hookar o Account;
   * Um *domain event* e seu *handler* na camada **Application**;
   * O repositório de `Account` na camada **Infrastructure**;
   * Qualquer configuração extra de DI ou *pipeline behavior* para garantir transacionalidade (e.g. Unit of Work, outbox, etc.).

Por favor, detalhe uma proposta completa — incluindo sugestões de bibliotecas (MediatR, EF Core, FluentValidation, etc.), convenções de nome, e exemplos de código nos pontos-chaves.

```