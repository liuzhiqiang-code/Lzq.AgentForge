import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { MaintenanceRecordApi } from '#/modules/mes/api/maintenance';
import { getEquipmentPage } from '#/modules/mes/api/equipment';

import { $t } from '#/locales';

const maintenanceTypeOptions: Record<number, string> = {
  1: $t('mes.equipment.maintenancePlan.maintenanceTypeDaily'),
  2: $t('mes.equipment.maintenancePlan.maintenanceTypeLevel1'),
  3: $t('mes.equipment.maintenancePlan.maintenanceTypeLevel2'),
  4: $t('mes.equipment.maintenancePlan.maintenanceTypeLevel3'),
  5: $t('mes.equipment.maintenancePlan.maintenanceTypePrecision'),
};

export function useSchema(): VbenFormSchema[] {
  return [
    {
      component: 'ApiSelect',
      componentProps: {
        api: async () => {
          const res = await getEquipmentPage({ pageNum: 1, pageSize: 1000 });
          return (res.list || []).map((item: any) => ({
            label: `${item.code} - ${item.name}`,
            value: item.id,
          }));
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: $t('mes.equipment.maintenanceRecord.equipmentPlaceholder'),
      },
      fieldName: 'equipmentId',
      label: $t('mes.equipment.maintenanceRecord.equipmentName'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        allowClear: false,
        options: Object.entries(maintenanceTypeOptions).map(([k, v]) => ({ label: v, value: Number(k) })),
        placeholder: '请选择保养类型',
      },
      fieldName: 'maintenanceType',
      label: $t('mes.equipment.maintenanceRecord.maintenanceType'),
      rules: 'required',
      defaultValue: 1,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'DatePicker',
      componentProps: {
        class: 'w-full',
        showTime: true,                     // ✅ 开启时间选择
        format: 'YYYY-MM-DD HH:mm:ss',      // ✅ 显示格式
        valueFormat: 'YYYY-MM-DD HH:mm:ss', // ✅ 输出格式
      },
      fieldName: 'maintenanceDate',
      label: $t('mes.equipment.maintenanceRecord.maintenanceDate'),
      rules: 'required',
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'InputNumber',
      componentProps:{
        class: 'w-full',
      },
      fieldName: 'durationMinutes',
      label: $t('mes.equipment.maintenanceRecord.durationMinutes'),
      defaultValue: 60,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'ApiSelect',
      componentProps: {
        // TODO: 替换为实际的用户API
        api: async () => {
          // 示例: const res = await getUserPage({ pageNum:1, pageSize:1000 });
          return [];
        },
        class: 'w-full',
        showSearch: true,
        allowClear: true,
        placeholder: $t('mes.equipment.maintenanceRecord.maintainerPlaceholder'),
      },
      fieldName: 'maintainerId',
      label: $t('mes.equipment.maintenanceRecord.maintainerName'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'content',
      label: $t('mes.equipment.maintenanceRecord.content'),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'result',
      label: $t('mes.equipment.maintenanceRecord.result'),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'problemsFound',
      label: $t('mes.equipment.maintenanceRecord.problemsFound'),
      formItemClass: 'col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'remark',
      label: $t('mes.equipment.maintenanceRecord.remark'),
      formItemClass: 'col-span-2',
    },
  ];
}

export function useColumns(): VxeTableGridOptions<MaintenanceRecordApi.MaintenanceRecordItem>['columns'] {
  return [
    { field: 'code', title: $t('mes.equipment.maintenanceRecord.code'), width: 140 },
    { field: 'equipmentName', title: $t('mes.equipment.maintenanceRecord.equipmentName'), width: 150 },
    {
      cellRender: { name: 'CellTag', attrs: { options: maintenanceTypeOptions } },
      field: 'maintenanceType',
      title: $t('mes.equipment.maintenanceRecord.maintenanceType'),
      width: 110,
    },
    { field: 'maintenanceDate', title: $t('mes.equipment.maintenanceRecord.maintenanceDate'), width: 130 },
    { field: 'maintainerName', title: $t('mes.equipment.maintenanceRecord.maintainerName'), width: 100 },
    { field: 'durationMinutes', title: $t('mes.equipment.maintenanceRecord.durationMinutes'), width: 100 },
    { field: 'content', title: $t('mes.equipment.maintenanceRecord.content'), minWidth: 150 },
    { field: 'result', title: $t('mes.equipment.maintenanceRecord.result'), minWidth: 120 },
    { field: 'problemsFound', title: $t('mes.equipment.maintenanceRecord.problemsFound'), minWidth: 150 },
    { field: 'remark', title: $t('mes.equipment.maintenanceRecord.remark'), width: 150 },
  ];
}

export { maintenanceTypeOptions };
