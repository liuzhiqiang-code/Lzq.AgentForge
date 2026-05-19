import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { WorkshopApi } from '#/modules/basadata/api/workshop';

import { getFactoryPage } from '#/modules/basadata/api/factory';
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
          const res = await getFactoryPage({ page: 1, pageSize: 999 });
          return (res?.items || []).map((item) => ({ label: item.name, value: item.id }));
        },
        placeholder: '请选择工厂',
        showSearch: true,
      },
      fieldName: 'factoryId',
      label: $t('basadata.workshop.factoryId'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'code',
      label: $t('basadata.workshop.code'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'name',
      label: $t('basadata.workshop.name'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Input',
      componentProps: { class: 'w-full' },
      fieldName: 'manager',
      label: $t('basadata.workshop.manager'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Switch',
      componentProps: {
        checkedValue: 1,                     // 选中时的值
        unCheckedValue: 0,                   // 未选中时的值
      },
      fieldName: 'status',
      label: $t('basadata.workshop.status'),
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
  ];
}

/**
 * 获取表格列配置
 */
export function useColumns(
  onActionClick?: OnActionClickFn<WorkshopApi.WorkshopItem>,
): VxeTableGridOptions<WorkshopApi.WorkshopItem>['columns'] {
  return [
    { field: 'code', title: $t('basadata.workshop.code') },
    { field: 'name', title: $t('basadata.workshop.name') },
    { field: 'factoryName', title: $t('basadata.workshop.factoryId')},
    { field: 'manager', title: $t('basadata.workshop.manager') },
    {
      cellRender: { name: 'CellTag', attrs: { options: { 0: '禁用', 1: '启用' } } },
      field: 'status',
      title: $t('basadata.workshop.status'),
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
      title: $t('basadata.workshop.operation'),
      width: 180,
    },
  ];
}
