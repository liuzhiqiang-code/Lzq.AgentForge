# 04 - JWT 认证参考

> 来源: lzq-extensions-jwt | 状态: ✅ 已验证

## Token 验证参数

| 验证项 | 默认行为 |
|---|---|
| Issuer | `ValidateIssuer = true` |
| Audience | `ValidateAudience = true` |
| 生存期 | `ValidateLifetime = true` |
| 签名密钥 | `ValidateIssuerSigningKey = true` |
| 时钟偏差 | `ClockSkew = 5分钟` |

## Claims 生成

`JwtService.GenerateToken` 生成固定 Claims：
- UserId, UserName, Roles, Email, Sex, TenantId, Sid, datetime

## 常见问题

| 问题 | 原因 | 解决 |
|---|---|---|
| 401 验证失败 | Token 过期或密钥不匹配 | 检查 Authorization 头和 SecurityKey |
| ICurrentUser 字段为空 | Token 未包含对应 Claims | 确保签发时使用 CurrentUser 完整信息 |
| 非 HTTP 环境 | 无 HttpContext | 手动构造 `new CurrentUser().SetUserId("bot")` |
