var chart = () => {

    function template(data) {
        return data.html;
    }
    var p1 = () => {
        $("#PharmacyNameFilter").select2();
        $.get('/DisplayOrders/GetAllPharmacy', (done) => {
            for (var i = 0; i < done.results.length; i++) {
                $('#PharmacyNameFilter').append($('<option>', {
                    value: done.results[i].id,
                    text: done.results[i].text,
                }));
            }
        });

        $("body").on("change","#PharmacyNameFilter", function (e) {
            var select_val = $("#PharmacyNameFilter").val();
            debugger;
            if (select_val != null) {
                $.get('/CustomersAdmin/GetStatusForPharmacy', { id: select_val })
                    .done(function (data) {
                        data = JSON.parse(data);
                        var com = 0; var pend = 0; var cancle = 0;
                        for (let i in data) {
                            if (data[i] == 1)
                                pend++;
                            else if (data[i] == 3)
                                cancle++;
                            else if (data[i] == 2)
                                com++;
                        }
                        var dom = document.getElementById('container1');
                        var myChart = echarts.init(dom, null, { renderer: 'canvas', useDirtyRect: false });
                        var data = [[
                            {
                                value: pend, name: 'pending', label: {
                                    normal: {
                                        formatter: '{b} : {c}',
                                        position: 'outside' //inside
                                    }
                                }
                            },
                            {
                                value: com, name: 'completed', label: {
                                    normal: {
                                        formatter: '{b} : {c}',
                                        position: 'outside' //inside
                                    }
                                }
                            },
                            {
                                value: cancle, name: 'canceled', label: {
                                    normal: {
                                        formatter: '{b} : {c}',
                                        position: 'outside' //inside
                                    }
                                }
                            },
                            { value: 0, name: 'No orders', itemStyle: { color: 'lightgray' }, label: { show: false } }
                            ,
                        ],
                        [{ value: 0, name: 'pending', label: { show: false } },
                        { value: 0, name: 'completed', label: { show: false } },
                        { value: 0, name: 'canceled', label: { show: false } }, { value: 1, name: 'No orders', itemStyle: { color: 'lightgray' } }]];

                        var option;
                        option = {
                            title: { text: $("#PharmacyNameFilter option:selected").text(), left: 'center' },
                            tooltip: {
                                trigger: 'item',
                            },
                            legend: {
                                orient: 'vertical',
                                position: 'labeled',
                                left: 'left',
                                data: ['pending', 'completed', 'canceled', 'No orders'],
                                formatter: name => {
                                    var series = myChart.getOption().series[0];
                                    var value = series.data.filter(row => row.name === name)[0].value
                                    return name + '    ' + value;
                                }

                            },
                            series: [
                                {
                                    name: 'Status',
                                    type: 'pie',
                                    radius: '50%',
                                    data: data[0],
                                    emphasis: {
                                        itemStyle: {
                                            shadowBlur: 10,
                                            shadowOffsetX: 0,
                                            shadowColor: 'rgba(0, 0, 0, 0.5)'
                                        }
                                    }
                                }
                            ]
                        };
                        var arr = [pend, com, cancle];
                        for (let i in arr) {
                            if (arr[i] == 0)
                                data[0][i]['label'] = { show: false };
                        }
                        if (com == 0 && pend == 0 && cancle == 0) {
                            option.series[0].data = data[1];
                        }

                        if (option && typeof option === 'object') {
                            myChart.setOption(option);
                        }
                        window.addEventListener('resize', myChart.resize);
                    });
            }
        });
    }
    return {
        inicOnce: () => {
            p1();
        },
    }
}

$(document).ready(() => {
    chart().inicOnce();
});