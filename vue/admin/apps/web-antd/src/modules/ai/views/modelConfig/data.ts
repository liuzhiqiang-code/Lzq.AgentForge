import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { ModelConfigApi } from '#/modules/ai/api/modelConfig';
import { getApiKeyList } from '#/modules/ai/api/apiKey';

import { z } from '#/adapter/form';
import { $t } from '#/locales';

/**
 * 获取编辑表单的字段配置
 * 注意：如果没有使用 $t() 翻译，需要手动 export 一个数组
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      fieldName: 'apiKeyId',
      label: $t('ai.modelConfig.apiKeyId'),
      componentProps: {
        api: async () => {
          const res = await getApiKeyList();
          return res.map((item) => ({
            label: item.keyName,
            value: item.id,
          }));
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: '请选择 API Key',
      },
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'configName',
      label: $t('ai.modelConfig.configName'),
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'displayModelName',
      label: $t('ai.modelConfig.displayModelName'),
    },
    {
      component: 'InputNumber',
      fieldName: 'contextLength',
      label: $t('ai.modelConfig.contextLength'),
      componentProps: {
        class: 'w-full',
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
      label: $t('ai.modelConfig.isEnabled'),
    },
  ];
}

/**
 * 获取表格列配置
 * @description 使用函数形式返回数组，而不是 export 一个 Array 变量，为了更好的代码提示
 * @param onActionClick 表格操作按钮点击事件
 */
export function useColumns(
  onActionClick?: OnActionClickFn<ModelConfigApi.ModelConfig>,
): VxeTableGridOptions<ModelConfigApi.ModelConfig>['columns'] {
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
      title: $t('ai.apiKey.keyName'),
    },
    {
      align: 'center',
      field: 'configName',
      title: $t('ai.modelConfig.configName'),
    },
    {
      align: 'center',
      field: 'displayModelName',
      title: $t('ai.modelConfig.displayModelName'),
    },
    {
      align: 'center',
      field: 'contextLength',
      title: $t('ai.modelConfig.contextLength'),
    },
    {
      align: 'center',
      field: 'isEnabled',
      title: $t('ai.modelConfig.isEnabled'),
      slots: { default: 'isEnabled' },
    },
    {
      align: 'center',
      cellRender: {
        attrs: {
          nameField: 'name',
          onClick: onActionClick,
        },
        name: 'CellOperation',
        options: ['edit'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('ai.modelConfig.operation'),
      width: 200,
    },
  ];
}
