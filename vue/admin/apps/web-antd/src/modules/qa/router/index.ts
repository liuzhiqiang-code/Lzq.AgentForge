import type { RouteRecordRaw } from 'vue-router';

import { $t } from '#/locales';

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:shield-check-outline',
      order: 1020,
      title: $t('qa.title'),
    },
    name: 'QA',
    path: '/qa',
    children: [
      {
        path: '/qa/qcorder',
        name: 'QCOrder',
        meta: {
          icon: 'mdi:clipboard-check-outline',
          title: $t('qa.qcorder.title'),
        },
        component: () => import('#/modules/qa/views/qcorder/list.vue'),
      },
      {
        path: '/qa/defect',
        name: 'Defect',
        meta: {
          icon: 'mdi:alert-circle-outline',
          title: $t('qa.defect.title'),
        },
        component: () => import('#/modules/qa/views/defect/list.vue'),
      },
    ],
  },
];

export default routes;
