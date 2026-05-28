<script lang="ts" setup>
import type { EchartsUIType } from '@vben/plugins/echarts';

import { Page, useVbenModal } from '@vben/common-ui';
import { onMounted, ref } from 'vue';

import { EchartsUI, useEcharts } from '@vben/plugins/echarts';

import { $t } from '#/locales';

import OutputCard from './components/OutputCard.vue';
import CompletionCard from './components/CompletionCard.vue';
import EquipmentCard from './components/EquipmentCard.vue';
import DefectRateCard from './components/DefectRateCard.vue';

import {
  getProductionOutput,
  getCompletionRate,
  getDefectRateStatistics,
  getEquipmentStatusOverview,
} from '#/modules/mes/api';

/**
 * 看板主页面
 * 布局：4卡片 + 图表（使用 VbenAdmin 自带 ECharts 组件）
 */

// 产量趋势图
const outputChartRef = ref<EchartsUIType>();
const { renderEcharts: renderOutputChart } = useEcharts(outputChartRef);

// 不良率趋势图
const defectChartRef = ref<EchartsUIType>();
const { renderEcharts: renderDefectChart } = useEcharts(defectChartRef);

// 当前选择的产线（可选，null 表示全部）
const selectedLineId = ref<number | undefined>(undefined);

onMounted(async () => {
  try {
    // 调用真实 API（可选传递 lineId）
    const outputData = await getProductionOutput({ lineId: selectedLineId.value });
    const completionData = await getCompletionRate({ lineId: selectedLineId.value });
    const defectData = await getDefectRateStatistics({ lineId: selectedLineId.value });
    const equipmentData = await getEquipmentStatusOverview();

    // TODO: 根据真实数据渲染图表
    console.log('API Data:', { outputData, completionData, defectData, equipmentData });
  } catch (error) {
    console.error('API 调用失败:', error);
  }

  // 产量趋势图（按产线）
  renderOutputChart({
    grid: {
      bottom: 0,
      containLabel: true,
      left: '1%',
      right: '1%',
      top: '2%',
    },
    series: [
      {
        barMaxWidth: 50,
        data: [3000, 4500, 2800, 5200, 3900, 4800, 5100],
        itemStyle: {
          color: '#1890ff',
        },
        name: $t('mes.dashboard.outputTrend'),
        type: 'bar',
      },
    ],
    tooltip: {
      axisPointer: {
        lineStyle: {
          width: 1,
        },
      },
      trigger: 'axis',
    },
    xAxis: {
      data: ['产线A', '产线B', '产线C', '产线D', '产线E', '产线F', '产线G'],
      type: 'category',
    },
    yAxis: {
      max: 6000,
      splitNumber: 4,
      type: 'value',
    },
  });

  // 不良率趋势图（按时间段）
  renderDefectChart({
    grid: {
      bottom: 0,
      containLabel: true,
      left: '1%',
      right: '1%',
      top: '2%',
    },
    series: [
      {
        areaStyle: {},
        data: [2.1, 1.8, 2.5, 1.5, 1.2, 1.8, 1.4, 1.1, 1.6, 1.3, 1.2, 1.0],
        itemStyle: {
          color: '#ff4d4f',
        },
        name: $t('mes.dashboard.defectTrend'),
        smooth: true,
        type: 'line',
      },
    ],
    tooltip: {
      axisPointer: {
        lineStyle: {
          width: 1,
        },
      },
      trigger: 'axis',
    },
    xAxis: {
      boundaryGap: false,
      data: Array.from({ length: 12 }).map((_item, index) => `${index + 1}月`),
      type: 'category',
    },
    yAxis: {
      max: 3.0,
      splitNumber: 4,
      type: 'value',
    },
  });
});
</script>

<template>
  <Page auto-content-height>
    <div class="dashboard-container p-4">
      <!-- 第一行：4个统计卡片 -->
      <div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4 mb-4">
        <OutputCard :line-id="selectedLineId" />
        <CompletionCard :line-id="selectedLineId" />
        <EquipmentCard />
        <DefectRateCard :line-id="selectedLineId" />
      </div>

      <!-- 第二行：产量趋势图 + 不良率趋势图 -->
      <div class="grid grid-cols-1 gap-4 lg:grid-cols-2 mb-4">
        <div class="bg-white rounded-lg p-4 shadow-sm">
          <h3 class="text-lg font-medium mb-4">
            {{ $t('mes.dashboard.outputTrend') }} ({{ $t('mes.dashboard.byLine') }})
          </h3>
          <EchartsUI ref="outputChartRef" class="h-64" />
        </div>

        <div class="bg-white rounded-lg p-4 shadow-sm">
          <h3 class="text-lg font-medium mb-4">
            {{ $t('mes.dashboard.defectTrend') }} ({{ $t('mes.dashboard.byTimePeriod') }})
          </h3>
          <EchartsUI ref="defectChartRef" class="h-64" />
        </div>
      </div>
    </div>
  </Page>
</template>

<style scoped>
.dashboard-container {
  min-height: calc(100vh - 120px);
}
</style>
