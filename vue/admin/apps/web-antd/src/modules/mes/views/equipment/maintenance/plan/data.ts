import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { MaintenancePlanApi } from '#/modules/mes/api/maintenance';
import { getEquipmentPage } from '#/modules/mes/api/equipment';

import { $t } from '#/locales';

const maintenanceTypeOptions = [
  { value: 1, label: $t('mes.equipment.maintenancePlan.maintenanceTypeDaily'), color: 'default' },
  { value: 2, label: $t('mes.equipment.maintenancePlan.maintenanceTypeLevel1'), color: 'processing' },
  { value: 3, label: $t('mes.equipment.maintenancePlan.maintenanceTypeLevel2'), color: 'warning' },
  { value: 4, label: $t('mes.equipment.maintenancePlan.maintenanceTypeLevel3'), color: 'error' },
  { value: 5, label: $t('mes.equipment.maintenancePlan.maintenanceTypePrecision'), color: 'success' },
];

const statusOptions = [
  { value: 0, label: $t('mes.equipment.maintenancePlan.statusPending'), color: 'default' },
  { value: 1, label: $t('mes.equipment.maintenancePlan.statusInProgress'), color: 'processing' },
  { value: 2, label: $t('mes.equipment.maintenancePlan.statusCompleted'), color: 'success' },
  { value: 3, label: $t('mes.equipment.maintenancePlan.statusDelayed'), color: 'warning' },
  { value: 4, label: $t('mes.equipment.maintenancePlan.statusCancelled'), color: 'error' },
];

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        // ✅ 直接使用后端接口，自动加载数据
        api: async () => {
          const res = await getEquipmentPage({ pageNum: 1, pageSize: 1000 });
          return (res.items || []).map((item: any) => ({
            label: `${item.code} - ${item.name}`,
            value: item.id,
          }));
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: $t('mes.equipment.maintenancePlan.equipmentPlaceholder'),
      },
      fieldName: 'equipmentId',
      label: $t('mes.equipment.maintenancePlan.equipmentId'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Input',
      componentProps:{
        class: 'w-full',
      },
      fieldName: 'name',
      label: $t('mes.equipment.maintenancePlan.name'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Select',
      componentProps: {
        allowClear: false,
        options: maintenanceTypeOptions.map(({ label, value }) => ({ label, value })),
        placeholder: $t('mes.equipment.maintenancePlan.maintenanceTypePlaceholder'),
        class: 'w-full',
      },
      fieldName: 'maintenanceType',
      label: $t('mes.equipment.maintenancePlan.maintenanceType'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps: { 
        placeholder: $t('mes.equipment.maintenancePlan.cycleDaysPlaceholder'),
        class: 'w-full',
       },
      fieldName: 'cycleDays',
      label: $t('mes.equipment.maintenancePlan.cycleDays'),
      defaultValue: 30,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        format: 'YYYY-MM-DD HH:mm:ss',      // ✅ 显示格式
        valueFormat: 'YYYY-MM-DD HH:mm:ss', // ✅ 输出格式
      },
      fieldName: 'planDate',
      label: $t('mes.equipment.maintenancePlan.planDate'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps: { 
        placeholder: $t('mes.equipment.maintenancePlan.durationMinutesPlaceholder'),
        class: 'w-full',
      },
      fieldName: 'durationMinutes',
      label: $t('mes.equipment.maintenancePlan.durationMinutes'),
      defaultValue: 60,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      fieldName: 'content',
      label: $t('mes.equipment.maintenancePlan.content'),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Textarea',
      fieldName: 'standard',
      label: $t('mes.equipment.maintenancePlan.standard'),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Textarea',
      fieldName: 'remark',
      label: $t('mes.equipment.maintenancePlan.remark'),
      formItemClass: 'col-span-2',
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<MaintenancePlanApi.MaintenancePlanItem>,
): VxeTableGridOptions<MaintenancePlanApi.MaintenancePlanItem>['columns'] {
  return [
    { field: 'code', title: $t('mes.equipment.maintenancePlan.code'), width: 140 },
    { field: 'name', title: $t('mes.equipment.maintenancePlan.name'), width: 180 },
    { field: 'equipmentName', title: $t('mes.equipment.maintenancePlan.equipmentName'), width: 150 },
    {
      cellRender: { name: 'CellTag', options: maintenanceTypeOptions },
      field: 'maintenanceType',
      title: $t('mes.equipment.maintenancePlan.maintenanceType'),
      width: 110,
    },
    { field: 'planDate', title: $t('mes.equipment.maintenancePlan.planDate'), width: 120 },
    {
      cellRender: { name: 'CellTag', options: statusOptions },
      field: 'status',
      title: $t('mes.equipment.maintenancePlan.status'),
      width: 100,
    },
    { field: 'responsibleName', title: $t('mes.equipment.maintenancePlan.responsibleName'), width: 100 },
    { field: 'durationMinutes', title: $t('mes.equipment.maintenancePlan.durationMinutes'), width: 110 },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'name', onClick: onActionClick },
        name: 'CellOperation',
        options:['edit', 'delete'],
        // options: (row: MaintenancePlanApi.MaintenancePlanItem) => {
        //   const ops: string[] = [];
        //   if (row.status === 0) { ops.push('edit', 'delete'); }
        //   return ops;
        // },
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('mes.equipment.maintenancePlan.operation'),
      width: 160,
    },
  ];
}
