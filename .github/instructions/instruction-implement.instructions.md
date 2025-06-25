Você é um assistente especializado em ASP.NET Core e MediatR. Preciso que você gere o código completo para consolidar o enforcement de sessão de jogo somente via PipelineBehavior, removendo qualquer middleware HTTP que faça esse trabalho. Além disso:

1. **Sliding-only expiration**  
   - Refatore o serviço de sessão (`IGameSessionService` / `GameSessionService`) para usar somente `SlidingExpiration` no cache (ou, opcionalmente, regravar manualmente a chave a cada requisição válida para renovar o TTL).  
   - Remova a `AbsoluteExpirationRelativeToNow`.

2. **Unificar enforcement**  
   - Garanta que TODO acesso a comandos ou queries que tenham o atributo `[RequireGameSession]` seja validado somente dentro de um único `IPipelineBehavior<TRequest, TResponse>`.  
   - Não utilize middleware de autorização nem filtros em controllers para esse propósito.

3. **Respeitar `RequireGameSessionAttribute`**  
   - Atualize o `RequireGameSessionAttribute` para expor:
     ```csharp
     public bool AllowExpiredSession { get; set; } = false;
     public int MinimumLevel { get; set; } = 0;
     ```
   - No `GameSessionBehavior`, para cada `TRequest` com o atributo:
     - Se `AllowExpiredSession == false`, exija sessão válida (chave presente e não expirada).  
     - Se `AllowExpiredSession == true`, permita retornar informações de leitura mesmo se a sessão já expirou (mas não permita comandos de escrita).  
     - Se `MinimumLevel > 0`, recupere o perfil do jogador (via `IGameSessionService.GetProfileAsync`) e lance `ForbiddenException` se o nível for inferior ao mínimo.

4. **Comunicar TTL ao cliente**  
   - Após cada requisição válida que renove o TTL, injete no `HttpResponse` um header `X-Game-Session-Expires-In` com o número de segundos até a sessão expirar.

5. **Métricas de sessão**  
   - No `GameSessionService`, adicione contadores (por exemplo, usando `ILogger` ou `Meter` do `System.Diagnostics.Metrics`) para medir:
     - Quantas sessões são criadas.  
     - Quantas são renovadas.  
     - Quantas são expiradas.  
     - Quantas são revogadas manualmente.

Por fim, gere o código completo das classes e da configuração em `Program.cs` para registrar o `GameSessionBehavior`, o serviço de sessão atualizado e as métricas.  
