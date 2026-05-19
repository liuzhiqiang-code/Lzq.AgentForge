<script lang="ts" setup>
import type { EchartsUIType } from '@vben/plugins/echarts';
import { onMounted, ref } from 'vue';
import { EchartsUI, useEcharts } from '@vben/plugins/echarts';
import { getAgentUsageRanking } from '#/api/dashboard/analytics';

const chartRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(chartRef);

async function initChart() {
  const data = await getAgentUsageRanking({ top: 5 });
  
  renderEcharts({
    tooltip: { trigger: 'axis' },
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    xAxis: { type: 'value' },
    yAxis: { 
      type: 'category', 
      data: data.map(item => item.name),
      inverse: true // 让第一名在上方
    },
    series: [
      {
        name: '调用次数',
        type: 'bar',
        data: data.map(item => item.count),
        itemStyle: {
          color: '#5ab1ef',
          borderRadius: [0, 4, 4, 0]
        },
        showBackground: true,
        backgroundStyle: { color: 'rgba(180, 180, 180, 0.1)' }
      }
    ]
  });
}

onMounted(initChart);
</script>

<template>
  <EchartsUI ref="chartRef" />
</template>
