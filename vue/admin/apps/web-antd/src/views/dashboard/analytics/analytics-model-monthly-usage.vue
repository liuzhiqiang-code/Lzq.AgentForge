<script lang="ts" setup>
import type { EchartsUIType } from '@vben/plugins/echarts';
import { onMounted, ref } from 'vue';
import { EchartsUI, useEcharts } from '@vben/plugins/echarts';
import { getModelUsageMonthly } from '#/api/dashboard/analytics';

const chartRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(chartRef);

onMounted(async () => {
  const { months, values } = await getModelUsageMonthly();

  renderEcharts({
    grid: { bottom: 0, containLabel: true, left: '1%', right: '1%', top: '2%' },
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'category', data: months },
    yAxis: { type: 'value' },
    series: [
      {
        name: '模型调用次数',
        type: 'bar',
        data: values,
        itemStyle: { color: '#5ab1ef' },
        barMaxWidth: 40,
      },
    ],
  });
});
</script>

<template>
  <EchartsUI ref="chartRef" />
</template>
