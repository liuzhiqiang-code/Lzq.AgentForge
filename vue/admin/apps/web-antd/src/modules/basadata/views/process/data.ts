import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { ProcessApi } from '#/modules/basadata/api/process';

import { getLinePage } from '#/modules/basadata/api/line';
import { $t } from '#/locales';

/**
 * 获取编辑表单的字段配置
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        allowClear: true,
        api: async () => {
          const res = await getLinePage({ page: 1, pageSize: 999 });
          return (res?.items || []).map((item) => ({ label: item.name, value: item.id }));
        },
        placeholder: '请选择产线',
        showSearch: true,
      },
      fieldName: 'lineId',
      label: $t('basadata.process.lineId'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('basadata.process.code'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('basadata.process.name'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps: { class: 'w-full' },
      fieldName: 'sequence',
      label: $t('basadata.process.sequence'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps: { min: 0, step: 0.1, class: 'w-full' },
      fieldName: 'standardHours',
      label: $t('basadata.process.standardHours'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Switch',
      componentProps: {
        checkedValue: 1,                     // 选中时的值
        unCheckedValue: 0,                   // 未选中时的值
      },
      fieldName: 'status',
      label: $t('basadata.process.status'),
      defaultValue: 1,
      formItemClass: 'col-span-2',
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns(
  onActionClick?: OnActionClickFn<ProcessApi.ProcessItem>,
): VxeTableGridOptions<ProcessApi.ProcessItem>['columns'] {
  return [
    { field: 'code', title: $t('basadata.process.code') },
    { field: 'name', title: $t('basadata.process.name')},
    { field: 'lineName', title: $t('basadata.process.lineId') },
    { field: 'sequence', title: $t('basadata.process.sequence') },
    { field: 'standardHours', title: $t('basadata.process.standardHours') },
    {
      cellRender: { name: 'CellTag', attrs: { options: { 0: '停用', 1: '启用' } } },
      field: 'status',
      title: $t('basadata.process.status'),
    },
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
      title: $t('basadata.process.operation'),
      width: 180,
    },
  ];
}
