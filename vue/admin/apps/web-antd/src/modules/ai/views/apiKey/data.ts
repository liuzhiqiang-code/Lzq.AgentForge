import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { ApiKeyApi } from '#/modules/ai/api/apiKey';
import { getAvailableModels } from '#/modules/ai/api/apiKey';

// import { z } from '#/adapter/form';
import { $t } from '#/locales';

/**
 * 厂商选项配置
 */
export interface ProviderItem {
  value: number;
  label: string;
  icon?: string;  // 厂商图标 URL
  baseUrl?: string; // API 基础地址
}

export const providers: ProviderItem[] = [
  {
    value: 1,
    label: 'DeepSeek',
    icon: 'https://www.deepseek.com/favicon.ico',
    baseUrl: 'https://api.deepseek.com/v1',
  },
  {
    value: 2,
    label: 'SiliconFlow',
    icon: 'https://www.siliconflow.cn/favicon.ico', // 硅基流动图标
    baseUrl: 'https://api.siliconflow.cn/v1',
  },
  // 可以在这里添加更多厂商
];

/**
 * 厂商映射：用于表格中显示厂商名称
 * 使用函数形式返回 Record，而不是 export 一个变量
 */
export const providerMap: Record<number, string> = Object.fromEntries(
  providers.map(p => [p.value, p.label])
);

/**
 * 获取编辑表单的字段配置
 * 注意：如果没有使用 $t() 翻译，需要手动 export 一个数组
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      fieldName: 'id',
      label: '',
      componentProps: {
        type: 'hidden',   // 隐藏字段
      },
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'provider',
      label: $t('ai.apiKey.provider'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'keyName',
      label: $t('ai.apiKey.keyName')
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'keyValue',
      label: $t('ai.apiKey.keyValue'),
      dependencies: {
        // 如果 id 存在，则禁用（编辑模式下 keyValue 不可修改）
        async componentProps(values) {
          return {
            disabled: !!values.id,
          };
        },
        triggerFields: ['id'],
      },
    },
    {
      component: 'Switch',
      componentProps: {
        class: 'w-auto',
        checkedValue: 1,                     // 选中时的值
        unCheckedValue: 0,                   // 未选中时的值
      },
      fieldName: 'isEnabled',
      label: $t('ai.apiKey.isEnabled')
    },
    {
      component: 'CheckboxGroup',          // 多选组件
      fieldName: 'selectModel',
      label: $t('ai.apiKey.selectModel'),
      componentProps: {
        options: [],                       // 初始为空
      },
      // 根据 provider 和 keyValue 动态获取可选模型列表
      dependencies: {
        async componentProps(values) {
          const { id, provider, keyValue } = values;

          if (id && keyValue?.includes('*')) {
            return {}; // 如果是编辑模式且密钥已隐藏，不需要加载选项
          }

          // 否则请求 API 获取可用模型
          if (!provider || !keyValue) return { options: [] };
          try {
            const models = await getAvailableModels(provider, keyValue);
            return {
              options: models.map(m => ({ label: m, value: m })),
            };
          } catch {
            return { options: [] };
          }
        },
        triggerFields: ['provider', 'keyValue'],
      }
    }
  ];
}

/**
 * 获取表格列配置
 * @description 使用函数形式返回数组，而不是 export 一个 Array 变量，为了更好的代码提示
 * @param onActionClick 表格操作按钮点击事件
 */
export function useColumns(
  onActionClick?: OnActionClickFn<ApiKeyApi.ApiKey>,
): VxeTableGridOptions<ApiKeyApi.ApiKey>['columns'] {
  return [
    {
      align: 'center',
      field: 'provider',
      title: $t('ai.apiKey.provider'),
      slots: { default: 'provider' },
    },
    {
      align: 'center',
      field: 'keyName',
      title: $t('ai.apiKey.keyName')
    },
    {
      align: 'center',
      field: 'keyValue',
      title: $t('ai.apiKey.keyValue')
    },
    {
      align: 'center',
      field: 'isEnabled',
      title: $t('ai.apiKey.isEnabled'),
      slots: { default: 'isEnabled' },
    },

    // 操作按钮
    {
      align: 'center',
      cellRender: {
        attrs: {
          nameField: 'name',
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: [
          'edit', // 编辑按钮
          {
            code: 'delete', // 删除按钮
            disabled: (row: ApiKeyApi.ApiKey) => {
              return !!(row.children && row.children.length > 0);
            },
          },
        ],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('ai.apiKey.operation'),
      width: 200,
    },
  ];
}
