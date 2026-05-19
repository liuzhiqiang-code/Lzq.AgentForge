# 代码审查清单 & 错误处理规范

## 代码审查清单（每次提交前逐项检查）

```
☐ 所有 import 来源是否都在白名单内（@vben/* / ant-design-vue / #/）？
☐ 路由配置是否使用了 Vben Admin 的标准方式（name + path + meta + component）？
☐ API 请求是否使用了封装好的 requestClient 实例？
☐ 是否有硬编码的魔法数字或字符串？
☐ TypeScript 类型是否完整、无 any（除了 Recordable<any> 等已知模式）？
☐ 样式是否使用了项目统一的 CSS 变量或 Tailwind 类？
☐ 多语言文本是否使用了 $t() 函数？
☐ 错误处理是否符合规范？
☐ 加载状态是否有处理？
☐ Switch 组件是否配置了 checkedValue/unCheckedValue（后端用 0/1 整数枚举）？
☐ DatePicker 是否包含 class: 'w-full' + format + valueFormat？
☐ ApiSelect 是否使用了组件本身而非 Select + options:[] 反模式？
☐ 所有表单组件是否配置了 class: 'w-full'？
☐ 中文内容正确性：文件中的中文注释、模板文本是否出现典型乱码（如 瀵煎叆、鏂板缓 等）？
```

## 错误处理规范

| 错误类型 | 处理方式 |
|---------|---------|
| API 错误 | requestClient 已有统一错误拦截（message.error），无需额外处理 |
| 表单校验 | 使用 VbenFormSchema 的 rules 字段（z.string() / 'required'） |
| 加载状态 | 弹窗用 modalApi.lock() / drawerApi.lock()，页面用 Spin 或 loading ref |
| 空状态 | 表格自动处理空数据，详情页可用 ant-design-vue 的 Empty 组件 |
| 操作确认 | 使用 Modal.confirm 封装为 Promise（参考 role/list.vue 的 confirm 函数） |
| 操作反馈 | 使用 message.success() / message.error()（参考 dept/list.vue） |
| 表单提交 | onConfirm 中先 valid = await formApi.validate()，再 lock → getValues → API → close |

## 模块化目录结构规范

```
src/modules/{ModuleName}/
├── index.ts                    # 模块入口，导出路由配置
├── api/                        # API 文件
│   └── index.ts
├── views/                      # 页面视图
│   └── {PageName}/
│       ├── list.vue
│       ├── detail.vue
│       ├── data.ts
│       └── modules/
│           └── form.vue
├── router/
│   └── index.ts                # 路由配置，export default routes
└── locales/
    └── zh-CN.json              # 国际化文件（目录结构: locales/zh-CN/{module}.json）
```

## API 规范

```typescript
import { requestClient } from '#/api/request';

export namespace XxxApi {
  export interface XxxItem {
    id: string;
    name: string;
    // ...
  }
}

async function getXxxList(params?: Recordable<any>) {
  return requestClient.get<Array<XxxApi.XxxItem>>('/mes/xxx/list', { params });
}

async function createXxx(data: Partial<XxxApi.XxxItem>) {
  return requestClient.post('/mes/xxx', data);
}

async function updateXxx(id: string, data: Partial<XxxApi.XxxItem>) {
  return requestClient.put(`/mes/xxx/${id}`, data);
}

async function deleteXxx(id: string) {
  return requestClient.delete(`/mes/xxx/${id}`);
}

export { createXxx, deleteXxx, getXxxList, updateXxx };
```

**关键规则**：
- `requestClient` 的 `baseURL` 已包含 `/api/v1`，前端调用时不要再加此前缀
- GET 请求参数用 `{ params }` 传递
- 响应默认 `responseReturn: 'data'`，即直接返回后端 data 字段内容

## 路由规范

模块路由文件 `router/index.ts`：

```typescript
import type { RouteRecordRaw } from 'vue-router';
import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ion:settings-outline',  // Iconify 图标名
      order: 1000,
      title: $t('module.title'),
    },
    name: 'ModuleName',
    path: '/module',
    children: [
      {
        path: '/module/list',
        name: 'ModuleList',
        meta: { icon: 'mdi:list', title: $t('module.list') },
        component: () => import('#/modules/{module}/views/{page}/list.vue'),
      },
    ],
  },
];
export default routes;
```

在 `src/router/routes/index.ts` 中手动注册：

```typescript
import { basadataRoutes } from '#/modules/basadata';

const moduleRoutes: RouteRecordRaw[] = [
  ...basadataRoutes,
];

const accessRoutes = [...dynamicRoutes, ...moduleRoutes, ...staticRoutes];
```
