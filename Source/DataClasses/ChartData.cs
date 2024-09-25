using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Source.DataClasses {
public class ChartData {
public static string TriangleConfig =
@"{
    type: 'radar',
    data: {
        labels: [],
        datasets: []
    },
    options: {
        legend: {
            display: [],
            labels: {
                fontSize: 14,
                fontStyle: 'bold',
                fontColor: 'rgba(115, 158, 231, 1)',
            },
        },
        scale: {
            pointLabels: {
                fontSize: 13,
                fontColor: 'rgba(95, 138, 211, 1)',
                fontStyle: 'bold'
            },
            rAxis: {
                color: 'blue',
                ticks: {
                    display: false
                }
            },
            ticks: {
                min: 0,
                max: [],
                stepSize: [],
                fontColor: 'blue',
                display: false
            },
            gridLines: {
                color: 'rgb(128,128,128)'
            },
            angleLines: {
                color: 'rgb(128,128,128)'
            }
        }
    }
}";

public static string VersusConfig =
@"{
    type: 'radar',
    data: {
        labels: ['APM', 'PPS', 'VS', 'APP', 'DS/Second', 'DS/Piece', 'APP+DS/Piece', 'VS/APM', 'Cheese Index', 'Garbage Effi.'],
        datasets: []
    },
    options: {
        legend: {
            display: [],
            labels: {
                fontSize: 14,
                fontStyle: 'bold',
                fontColor: 'rgba(115, 158, 231, 1)',
            },
        },
        scale: {
            pointLabels: {
                fontSize: 13,
                fontColor: 'rgba(95, 138, 211, 1)',
                fontStyle: 'bold'
            },
            rAxis: {
                ticks: {
                    display: false
                }
            },
            ticks: {
                min: 0,
                max: 180,
                stepSize: 30,
                fontColor: 'blue',
                display: false
            },
            gridLines: {
              color: 'grey'
            },
            angleLines: {
              color: 'grey'
            }
        }
    }
}";

public static string TlHistoryConfig =
@"{
    type: 'line',
    data:
    {
        labels: [],
        datasets:
        [{
            label: [],
            data: [],
            pointRadius: 0,
        }]
    }
}";
} }
