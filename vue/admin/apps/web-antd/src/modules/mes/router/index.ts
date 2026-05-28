import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ion:server-outline',
      order: 1000,
      title: $t('mes.title'),
    },
    name: 'MES',
    path: '/mes',
    children: [
      {
        meta: {
          icon: 'ion:server-outline',
          title: $t('mes.basadata.title'),
        },
        name: 'MESBaseData',
        path: '/mes/basadata',
        children: [
          {
            path: '/mes/basadata/factory',
            name: 'MESFactory',
            meta: {
              icon: 'mdi:factory',
              title: $t('mes.basadata.factory.title'),
            },
            component: () => import('#/modules/mes/views/basadata/factory/list.vue'),
          },
          {
            path: '/mes/basadata/workshop',
            name: 'MESWorkshop',
            meta: {
              icon: 'mdi:warehouse',
              title: $t('mes.basadata.workshop.title'),
            },
            component: () => import('#/modules/mes/views/basadata/workshop/list.vue'),
          },
          {
            path: '/mes/basadata/line',
            name: 'MESLine',
            meta: {
              icon: 'mdi:sort-variant',
              title: $t('mes.basadata.line.title'),
            },
            component: () => import('#/modules/mes/views/basadata/line/list.vue'),
          },
          {
            path: '/mes/basadata/process',
            name: 'MESProcess',
            meta: {
              icon: 'mdi:cog-sync-outline',
              title: $t('mes.basadata.process.title'),
            },
            component: () => import('#/modules/mes/views/basadata/process/list.vue'),
          },
          {
            path: '/mes/basadata/material',
            name: 'MESMaterial',
            meta: {
              icon: 'mdi:package-variant-closed',
              title: $t('mes.basadata.material.title'),
            },
            component: () => import('#/modules/mes/views/basadata/material/list.vue'),
          },
        ],
      },
      {
        meta: {
          icon: 'mdi:clipboard-text-outline',
          title: $t('mes.workorder.title'),
        },
        name: 'MESWorkOrder',
        path: '/mes/workorder',
        children: [
          {
            path: '/mes/workorder/list',
            name: 'MESWorkOrderList',
            meta: {
              icon: 'mdi:clipboard-list-outline',
              title: $t('mes.workorder.workorder.title'),
            },
            component: () => import('#/modules/mes/views/workorder/workorder/list.vue'),
          },
          {
            path: '/mes/workorder/workreport',
            name: 'MESWorkReport',
            meta: {
              icon: 'mdi:clipboard-check-outline',
              title: $t('mes.workorder.workreport.title'),
            },
            component: () => import('#/modules/mes/views/workorder/workreport/list.vue'),
          },
        ],
      },
      {
        meta: {
          icon: 'mdi:shield-check-outline',
          title: $t('mes.qa.title'),
        },
        name: 'MESQA',
        path: '/mes/qa',
        children: [
          {
            path: '/mes/qa/qcorder',
            name: 'MESQCOrder',
            meta: {
              icon: 'mdi:clipboard-check-outline',
              title: $t('mes.qa.qcorder.title'),
            },
            component: () => import('#/modules/mes/views/qa/qcorder/list.vue'),
          },
          {
            path: '/mes/qa/defect',
            name: 'MESDefect',
            meta: {
              icon: 'mdi:alert-circle-outline',
              title: $t('mes.qa.defect.title'),
            },
            component: () => import('#/modules/mes/views/qa/defect/list.vue'),
          },
        ],
      },
      {
        meta: {
          icon: 'mdi:tools',
          title: $t('mes.equipment.title'),
        },
        name: 'MESEquipment',
        path: '/mes/equipment',
        children: [
          {
            path: '/mes/equipment/list',
            name: 'MESEquipmentList',
            meta: {
              icon: 'mdi:excavator',
              title: $t('mes.equipment.equipment.title'),
            },
            component: () => import('#/modules/mes/views/equipment/equipment/list.vue'),
          },
          {
            path: '/mes/equipment/inspection-plan',
            name: 'MESInspectionPlan',
            meta: {
              icon: 'mdi:clipboard-text-search-outline',
              title: $t('mes.equipment.inspectionPlan.title'),
            },
            component: () => import('#/modules/mes/views/equipment/inspection/plan/list.vue'),
          },
          {
            path: '/mes/equipment/inspection-record',
            name: 'MESInspectionRecord',
            meta: {
              icon: 'mdi:clipboard-check-outline',
              title: $t('mes.equipment.inspectionRecord.title'),
            },
            component: () => import('#/modules/mes/views/equipment/inspection/record/list.vue'),
          },
          {
            path: '/mes/equipment/maintenance-plan',
            name: 'MESMaintenancePlan',
            meta: {
              icon: 'mdi:wrench-clock-outline',
              title: $t('mes.equipment.maintenancePlan.title'),
            },
            component: () => import('#/modules/mes/views/equipment/maintenance/plan/list.vue'),
          },
          {
            path: '/mes/equipment/maintenance-record',
            name: 'MESMaintenanceRecord',
            meta: {
              icon: 'mdi:wrench-check-outline',
              title: $t('mes.equipment.maintenanceRecord.title'),
            },
            component: () => import('#/modules/mes/views/equipment/maintenance/record/list.vue'),
          },
          {
            path: '/mes/equipment/repair',
            name: 'MESRepair',
            meta: {
              icon: 'mdi:hammer-wrench',
              title: $t('mes.equipment.repair.title'),
            },
            component: () => import('#/modules/mes/views/equipment/repair/list.vue'),
          },
        ],
      },
      {
        meta: {
          icon: 'mdi:view-dashboard-outline',
          title: $t('mes.dashboard.title'),
        },
        name: 'MESDashboard',
        path: '/mes/dashboard',
        children: [
          {
            path: '/mes/dashboard/index',
            name: 'MESDashboardIndex',
            meta: {
              icon: 'mdi:monitor-dashboard',
              title: $t('mes.dashboard.overview'),
            },
            component: () => import('#/modules/mes/views/dashboard/index.vue'),
          },
        ],
      },
    ],
  },
];

export default routes;
