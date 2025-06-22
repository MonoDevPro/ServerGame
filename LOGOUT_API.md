# API de Logout/Revoke - Documentação

## Endpoint de Logout

O endpoint de logout permite invalidar refresh tokens de forma segura, revogando todas as sessões ativas do usuário.

### Endpoint
- **POST** `/logout` ou `/revoke`

### Parâmetros
```json
{
  "refreshToken": "string" // Refresh token que deve ser revogado
}
```

### Respostas

#### Sucesso (200 OK)
```json
{
  "message": "Logout realizado com sucesso"
}
```

#### Não autorizado (401 Unauthorized)
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401
}
```

### Como funciona

1. **Validação do Refresh Token**: O endpoint primeiro valida se o refresh token fornecido é válido e não expirou
2. **Identificação do Usuário**: Extrai o usuário associado ao refresh token
3. **Invalidação Global**: Atualiza o `SecurityStamp` do usuário, invalidando TODOS os tokens ativos (access tokens e refresh tokens)
4. **Logout de Cookies**: Remove qualquer sessão baseada em cookies
5. **Notificação**: Dispara evento de logout para auditoria e logging

### Exemplo de uso com cURL

```bash
curl -X POST https://api.gameserver.com/logout \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "CfDJ8M5...seu_refresh_token_aqui"
  }'
```

### Exemplo de uso com JavaScript

```javascript
async function logout(refreshToken) {
  try {
    const response = await fetch('/logout', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        refreshToken: refreshToken
      })
    });

    if (response.ok) {
      console.log('Logout realizado com sucesso');
      // Remover tokens do localStorage/sessionStorage
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      // Redirecionar para página de login
      window.location.href = '/login';
    } else {
      console.error('Erro no logout:', response.statusText);
    }
  } catch (error) {
    console.error('Erro na requisição de logout:', error);
  }
}
```

### Segurança

- **Invalidação Imediata**: Todos os tokens do usuário se tornam inválidos imediatamente
- **Proteção contra Replay**: Tokens invalidados não podem ser reutilizados
- **Auditoria**: Eventos de logout são registrados para auditoria
- **Cleanup Automático**: Sessões em cookies também são removidas

### Fluxo Recomendado

1. **Cliente** faz logout clicando no botão sair
2. **Cliente** envia o refresh token para `/logout`
3. **Servidor** invalida todos os tokens do usuário
4. **Cliente** remove tokens do armazenamento local
5. **Cliente** redireciona para página de login

### Considerações Importantes

- O endpoint aceita tanto `/logout` quanto `/revoke` (são equivalentes)
- Uma vez que o logout é realizado, TODOS os dispositivos/sessões do usuário precisarão fazer login novamente
- É importante sempre enviar o refresh token mais recente
- Após o logout, qualquer tentativa de uso dos tokens antigos resultará em erro 401
