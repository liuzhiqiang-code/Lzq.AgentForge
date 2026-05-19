import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { AgentManageApi } from '#/modules/ai/api/agentManage';
import { z } from '@vben/common-ui';
import { markRaw } from 'vue';

import { $t } from '#/locales';

/**
 * 获取编辑表单的字段配置
 * 注意：如果没有使用 $t() 翻译，需要手动 export 一个数组
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('ai.agentManage.name'),
      rules: 'required',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'description',
      label: $t('ai.agentManage.description'),
    },
    {
      // 文本域组件
      component: 'Textarea',
      fieldName: 'instructions',
      label: $t('ai.agentManage.instructions'),
      rules: 'required',
      componentProps: {
        autoSize: { minRows: 3, maxRows: 6 },
        placeholder: $t('ai.agentManage.instructions'),
      },
    },
    {
      component: 'InputNumber',
      fieldName: 'temperature',
      label: $t('ai.agentManage.temperature'),
      componentProps: {
        class: 'w-full',
        min: 0,
        max: 2,
        step: 0.1,
        precision: 1,
      },
    },
    {
      component: 'InputNumber',
      fieldName: 'maxOutputTokens',
      label: $t('ai.agentManage.maxOutputTokens'),
      componentProps: {
        class: 'w-full',
        min: 1,
        max: 4096,
      },
    },
    {
      component: 'InputNumber',
      fieldName: 'topP',
      label: $t('ai.agentManage.topP'),
      componentProps: {
        class: 'w-full',
        min: 0,
        max: 1,
        step: 0.1,
        precision: 2,
      },
    },
    {
      component: 'InputNumber',
      fieldName: 'frequencyPenalty',
      label: $t('ai.agentManage.frequencyPenalty'),
      componentProps: {
        class: 'w-full',
        min: 0,
        max: 2,
        step: 0.1,
        precision: 1,
      },
    },
    {
      component: 'InputNumber',
      fieldName: 'presencePenalty',
      label: $t('ai.agentManage.presencePenalty'),
      componentProps: {
        class: 'w-full',
        min: 0,
        max: 2,
        step: 0.1,
        precision: 1,
      },
    },
    {
      component: 'ApiSelect',
      fieldName: 'selectedSkills',
      label: $t('ai.agentManage.selectedSkills'),
      componentProps: {
        class: 'w-full',
        mode: 'multiple',
        api: async () => {
          // TODO: 获取技能列表的 API
          return [];
        },
        placeholder: '请选择技能',
      },
    },
  ];
}

/**
 * 获取表格列配置
 * @param onActionClick 表格操作按钮点击事件
 */
export function useColumns(
  onActionClick?: OnActionClickFn<AgentManageApi.AgentManage>,
): VxeTableGridOptions<AgentManageApi.AgentManage>['columns'] {
  return [
    { field: 'name', title: $t('ai.agentManage.name') },
    { field: 'description', title: $t('ai.agentManage.description') },
    { field: 'temperature', title: $t('ai.agentManage.temperature') },
    { field: 'maxOutputTokens', title: $t('ai.agentManage.maxOutputTokens') },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'name', onClick: onActionClick },
        name: 'CellOperation',
        options: ['edit', 'delete'],
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('ai.agentManage.operation'),
      width: 150,
    },
  ];
}
