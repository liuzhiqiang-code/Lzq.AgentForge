import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { FactoryApi } from '#/modules/mes/api/factory';

import { $t } from '#/locales';

/**
 * 获取编辑表单的字段配置
 */
export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('mes.basadata.factory.code'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('mes.basadata.factory.name'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'address',
      label: $t('mes.basadata.factory.address'),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Switch',
      componentProps: {
        checkedValue: 1,                     // 选中时的值
        unCheckedValue: 0,                   // 未选中时的值
      },
      fieldName: 'status',
      label: $t('mes.basadata.factory.status'),
      defaultValue: 1,
      formItemClass: 'col-span-2',
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns(
  onActionClick?: OnActionClickFn<FactoryApi.FactoryItem>,
): VxeTableGridOptions<FactoryApi.FactoryItem>['columns'] {
  return [
    { field: 'code', title: $t('mes.basadata.factory.code'), width: 120 },
    { field: 'name', title: $t('mes.basadata.factory.name'), width: 200 },
    { field: 'address', title: $t('mes.basadata.factory.address'), minWidth: 200 },
    {
      cellRender: { name: 'CellTag', attrs: { options: { 0: '禁用', 1: '启用' } } },
      field: 'status',
      title: $t('mes.basadata.factory.status'),
      width: 100,
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
      title: $t('mes.basadata.factory.operation'),
      width: 180,
    },
  ];
}
