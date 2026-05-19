import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { VbenFormSchema } from '#/adapter/form';
import type { OnActionClickFn } from '#/adapter/vxe-table';
import type { RepairApi } from '#/modules/equipment/api/repair';
import { getEquipmentPage } from '#/modules/equipment/api/equipment';

import { $t } from '#/locales';

const repairTypeOptions: Record<number, string> = {
  1: $t('equipment.repair.repairTypeFault'),
  2: $t('equipment.repair.repairTypeScheduled'),
  3: $t('equipment.repair.repairTypeImprovement'),
  4: $t('equipment.repair.repairTypePreventive'),
};

const priorityOptions: Record<number, string> = {
  1: $t('equipment.repair.priorityUrgent'),
  2: $t('equipment.repair.priorityHigh'),
  3: $t('equipment.repair.priorityMedium'),
  4: $t('equipment.repair.priorityLow'),
};

const statusMap: Record<number, string> = {
  0: $t('equipment.repair.statusPending'),
  1: $t('equipment.repair.statusAssigned'),
  2: $t('equipment.repair.statusInProgress'),
  3: $t('equipment.repair.statusCompleted'),
  4: $t('equipment.repair.statusAccepted'),
  5: $t('equipment.repair.statusCancelled'),
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
        placeholder: $t('equipment.repair.equipmentPlaceholder'),
      },
      fieldName: 'equipmentId',
      label: $t('equipment.repair.equipmentName'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        allowClear: false,
        options: Object.entries(repairTypeOptions).map(([k, v]) => ({ label: v, value: Number(k) })),
        placeholder: '请选择维修类型',
      },
      fieldName: 'repairType',
      label: $t('equipment.repair.repairType'),
      rules: 'required',
      defaultValue: 3,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Select',
      componentProps: {
        class: 'w-full',
        allowClear: false,
        options: Object.entries(priorityOptions).map(([k, v]) => ({ label: v, value: Number(k) })),
        placeholder: '请选择优先级',
      },
      fieldName: 'priority',
      label: $t('equipment.repair.priority'),
      rules: 'required',
      defaultValue: 3,
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'description',
      label: $t('equipment.repair.description'),
      rules: 'required',
      formItemClass: 'col-span-2',
    },
    {
      component: 'ApiSelect',
      componentProps: {
        // TODO: 替换为实际的用户API
        api: async () => {
          // 示例: const res = await getUserPage({ pageNum:1, pageSize:1000 });
          return [];
        },
        showSearch: true,
        allowClear: true,
        placeholder: $t('equipment.repair.reporterPlaceholder'),
      },
      fieldName: 'reporterId',
      label: $t('equipment.repair.reporterName'),
      formItemClass: 'md:col-span-1 col-span-2',
    },
    {
      component: 'Textarea',
      componentProps: { class: 'w-full' },
      fieldName: 'remark',
      label: $t('equipment.repair.remark'),
      formItemClass: 'col-span-2',
    },
  ];
}

export function useColumns(
  onActionClick?: OnActionClickFn<RepairApi.RepairItem>,
): VxeTableGridOptions<RepairApi.RepairItem>['columns'] {
  return [
    { field: 'code', title: $t('equipment.repair.code'), width: 140 },
    { field: 'equipmentName', title: $t('equipment.repair.equipmentName'), width: 150 },
    {
      cellRender: { name: 'CellTag', attrs: { options: repairTypeOptions } },
      field: 'repairType',
      title: $t('equipment.repair.repairType'),
      width: 100,
    },
    { field: 'description', title: $t('equipment.repair.description'), minWidth: 160 },
    { field: 'reporterName', title: $t('equipment.repair.reporterName'), width: 100 },
    { field: 'reportTime', title: $t('equipment.repair.reportTime'), width: 150 },
    {
      cellRender: { name: 'CellTag', attrs: { options: priorityOptions } },
      field: 'priority',
      title: $t('equipment.repair.priority'),
      width: 80,
    },
    {
      cellRender: { name: 'CellTag', attrs: { options: statusMap } },
      field: 'status',
      title: $t('equipment.repair.status'),
      width: 100,
    },
    { field: 'repairUserName', title: $t('equipment.repair.repairUserName'), width: 100 },
    { field: 'repairStartTime', title: $t('equipment.repair.repairStartTime'), width: 150 },
    { field: 'repairEndTime', title: $t('equipment.repair.repairEndTime'), width: 150 },
    {
      align: 'center',
      cellRender: {
        attrs: { nameField: 'code', onClick: onActionClick },
        name: 'CellOperation',
        options: (row: RepairApi.RepairItem) => {
          const ops: string[] = [];
          switch (row.status) {
            case 0:
              ops.push('assign');
              break;
            case 1:
              ops.push('start', 'cancel');
              break;
            case 2:
              ops.push('complete');
              break;
            case 3:
              ops.push('accept');
              break;
          }
          return ops;
        },
      },
      field: 'operation',
      fixed: 'right',
      headerAlign: 'center',
      showOverflow: false,
      title: $t('equipment.repair.operation'),
      width: 200,
    },
  ];
}

export { priorityOptions, repairTypeOptions, statusMap };
