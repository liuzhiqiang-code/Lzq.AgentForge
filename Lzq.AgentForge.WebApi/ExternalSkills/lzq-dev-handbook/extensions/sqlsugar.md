# 08 - SqlSugar ORM 参考

> 来源: lzq-extensions-sqlsugar | 状态: ✅ 已验证

## 实体基类对比

| 基类 | 字段 | 适用场景 |
|---|---|---|
| `BaseFullEntity` | Id, Creator, CreationTime, Modifier, ModificationTime, IsDeleted | ✅ 大多数业务实体 |
| `BaseEntity` | Id, IsDeleted | 辅助表（当前版本中不存在，统一用 BaseFullEntity） |

## 雪花 ID 配置

```json
{
  "IdGeneratorOptions": {
    "WorkerId": 1  // 0~63，每个实例唯一
  }
}
```

## 数据库类型支持

`DbType`：MySql, SqlServer, Sqlite, Oracle, PostgreSQL

## AOP 审计机制

| 操作 | 自动填充 |
|---|---|
| 插入 | `CreationTime`=`ModificationTime`=当前时间<br>`Creator`=`Modifier`=当前用户(或"0") |
| 更新 | `ModificationTime`=当前时间<br>`Modifier`=当前用户 |
| 删除 | `IsDeleted`=`true`（逻辑删除） |

## 全局查询过滤器

自动添加 `WHERE is_deleted = false`

## 多数据库连接

通过 `ISqlSugarClient.AsTenant().ChangeDatabase(tag)` 切换

## 注意事项

- 必须在 `AddLzqSqlSugar` 前调用 `builder.AddLzqMasaAssembly()`
- WorkerId 必须唯一，避免 ID 冲突
- 列名和表名自动转为小写
- AOP 审计依赖 `ICurrentUser`，未注册时 Creator/Modifier 为 "0"
