import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:clipboard-text-outline',
      order: 1010,
      title: $t('workorder.title'),
    },
    name: 'WorkOrder',
    path: '/workorder',
    children: [
      {
        path: '/workorder/list',
        name: 'WorkOrderList',
        meta: {
          icon: 'mdi:clipboard-list-outline',
          title: $t('workorder.workorder.title'),
        },
        component: () => import('#/modules/workorder/views/workorder/list.vue'),
      },
      {
        path: '/workorder/workreport',
        name: 'WorkReport',
        meta: {
          icon: 'mdi:clipboard-check-outline',
          title: $t('workorder.workreport.title'),
        },
        component: () => import('#/modules/workorder/views/workreport/list.vue'),
      },
    ],
  },
];

export default routes;
