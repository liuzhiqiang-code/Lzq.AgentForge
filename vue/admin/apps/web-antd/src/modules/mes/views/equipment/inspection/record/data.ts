import type { VxeTableGridOptions } from '@vben/plugins/vxe-table';
import type { InspectionRecordApi } from '#/modules/mes/api/inspection';

import { $t } from '#/locales';

const resultMap: Record<number, string> = {
  0: $t('mes.equipment.inspectionRecord.resultNormal'),
  1: $t('mes.equipment.inspectionRecord.resultAbnormal'),
  2: $t('mes.equipment.inspectionRecord.resultNeedRepair'),
};

export function useColumns(): VxeTableGridOptions<InspectionRecordApi.InspectionRecordItem>['columns'] {
  return [
    { field: 'code', title: $t('mes.equipment.inspectionRecord.code'), width: 150 },
    { field: 'equipmentName', title: $t('mes.equipment.inspectionRecord.equipmentName'), width: 150 },
    { field: 'inspectDate', title: $t('mes.equipment.inspectionRecord.inspectDate'), width: 130 },
    {
      cellRender: { name: 'CellTag', attrs: { options: resultMap } },
      field: 'result',
      title: $t('mes.equipment.inspectionRecord.result'),
      width: 100,
    },
    { field: 'inspectorName', title: $t('mes.equipment.inspectionRecord.inspectorName'), width: 100 },
    { field: 'completedTime', title: $t('mes.equipment.inspectionRecord.completedTime'), width: 130 },
    { field: 'durationMinutes', title: $t('mes.equipment.inspectionRecord.durationMinutes'), width: 100 },
    { field: 'abnormalDesc', title: $t('mes.equipment.inspectionRecord.abnormalDesc'), minWidth: 150 },
    { field: 'remark', title: $t('mes.equipment.inspectionRecord.remark'), width: 150 },
  ];
}

export { resultMap };
