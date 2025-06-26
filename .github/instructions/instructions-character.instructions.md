
> “Preciso estender meu domínio de jogo adicionando uma nova entidade `Character` associada à minha entidade já existente `Account`. Cada `Account` deve poder ter até **3** personagens.
>
> **Requisitos**:
>
> 1. Defina a classe de domínio `Character` com propriedades:
>
>    * `Id` (Guid),
>    * `AccountId` (Guid),
>    * `Name` (string, 3–20 caracteres),
>    * `Class` (enum com pelo menos: Warrior, Mage, Rogue, Archer),
>    * `Level` (int, padrão 1),
>    * `Experience` (long, padrão 0),
>    * `CreatedAt` e `UpdatedAt` (DateTime).
> 2. Atualize `Account` para expor uma coleção `IReadOnlyCollection<Character>` e um método de negócio `AddCharacter(Character)` que:
>
>    * impede mais de 3 personagens,
>    * valida que o `AccountId` do `Character` bate com o `Id` da conta.
> 3. Configure o EF Core:
>
>    * `DbSet<Character>` no seu `GameDbContext`,
>    * mapeie a relação 1\:N com cascade delete,
>    * índice único em `(AccountId, Name)`.
> 4. Gere uma migração para criar a tabela `Characters` no banco.
>
> **Objetivo**: receber o código C# completo das classes de domínio, configurações de EF e comandos de migração, sem entrar ainda na camada de Application ou nos endpoints.”