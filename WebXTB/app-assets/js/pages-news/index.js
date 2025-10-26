window.onload = function () {

    getDataDonut();
    getDataStatisticNewsInWeek();
    getDataStatisticTopTargetInWeek();
    loadTagCloud();
    loadTopNewsSmooth();
};

function getDataDonut() {
    var isRtl = $('html').attr('data-textdirection') === 'rtl',
        chartColors = {
            donut: {
                series1: '#ffe700',
                series2: '#00d4bd',
                series3: '#826bf8',
                series4: '#2b9bf4',
                series5: '#FFA1A1',
                series6: '#000000',
                series7: '#FF9F43',
                series8: '#EA5455'
            }
        };

    $.ajax({
        url: '/Home/StatisticsTarget',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            // =============== DONUT CHART ===============
            if (response.series && response.series.length > 0) {
                var donutChartEl = document.querySelector('#donut-chart');

                // Dữ liệu donut: dùng series đầu tiên (vì bạn chỉ có 1 "AllTarget")
                var seriesData = response.series[0].data;
                var labelsData = response.labels;

                var donutChartConfig = {
                    chart: {
                        height: 315,
                        type: 'donut'
                    },
                    legend: {
                        show: true,
                        position: 'bottom'
                    },
                    labels: labelsData,
                    series: seriesData,
                    colors: [
                        chartColors.donut.series1,
                        chartColors.donut.series2,
                        chartColors.donut.series3,
                        chartColors.donut.series4,
                        chartColors.donut.series5,
                        chartColors.donut.series6,
                        chartColors.donut.series7,
                        chartColors.donut.series8
                    ],
                    dataLabels: {
                        enabled: true,
                        formatter: function (val, opt) {
                            return Math.round(val) + '%';
                        }
                    },
                    plotOptions: {
                        pie: {
                            donut: {
                                labels: {
                                    show: true,
                                    name: {
                                        fontSize: '2rem',
                                        fontFamily: 'Montserrat'
                                    },
                                    value: {
                                        fontSize: '1rem',
                                        fontFamily: 'Montserrat',
                                        formatter: function (val) {
                                            return Math.round(val);
                                        }
                                    },
                                    total: {
                                        show: true,
                                        label: 'Tổng',
                                        fontSize: '1.5rem',
                                        formatter: function (w) {
                                            return response.total ? response.total.toFixed(0) : 0;
                                        }
                                    }
                                }
                            }
                        }
                    },
                    responsive: [
                        {
                            breakpoint: 992,
                            options: { chart: { height: 380 } }
                        },
                        {
                            breakpoint: 576,
                            options: {
                                chart: { height: 320 },
                                plotOptions: {
                                    pie: {
                                        donut: {
                                            labels: {
                                                show: true,
                                                name: { fontSize: '1.5rem' },
                                                value: { fontSize: '1rem' },
                                                total: { fontSize: '1.5rem' }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    ]
                };

                if (typeof donutChartEl !== undefined && donutChartEl !== null) {
                    donutChartEl.innerHTML = '';
                    var donutChart = new ApexCharts(donutChartEl, donutChartConfig);
                    donutChart.render();
                }
            } else {
                document.querySelector('#donut-chart').innerHTML = '';
            }
        },
        error: function (xhr, status, error) {
            console.error('Lỗi lấy dữ liệu chart:', error);
        }
    });
}

function getDataStatisticNewsInWeek() {
    var isRtl = $('html').attr('data-textdirection') === 'rtl';
    var chartColors = {
        column: {
            website: '#28c76f',  // xanh lá
            youtube: '#ff9f43',  // cam
            total: '#ea5455',    // đỏ
            bg: '#ffffff'
        }
    };

    $.ajax({
        url: '/Home/StatisticNewsInWeek', // API mới trong controller
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.error) {
                console.error('Lỗi dữ liệu:', response.message);
                return;
            }

            // Hiển thị tổng nếu muốn
            if (response.total !== undefined) {
                $('#total-news').text('Tổng bài trong tuần: ' + response.total);
            }

            // Cấu hình biểu đồ
            var columnChartEl = document.querySelector('#column-chart'),
                columnChartConfig = {
                    chart: {
                        height: 280,
                        type: 'bar',
                        stacked: false,
                        parentHeightOffset: 0,
                        toolbar: {
                            show: false
                        }
                    },
                    plotOptions: {
                        bar: {
                            horizontal: false,
                            columnWidth: '80%',
                            borderRadius: 6
                        }
                    },
                    dataLabels: {
                        enabled: true,
                        style: {
                            fontSize: '11px',
                            colors: ['#fff']
                        }
                    },
                    legend: {
                        show: true,
                        position: 'top',
                        horizontalAlign: 'start'
                    },
                    colors: [
                        chartColors.column.website,
                        chartColors.column.youtube,
                        chartColors.column.total
                    ],
                    stroke: {
                        show: true,
                        width: 2,
                        colors: ['transparent']
                    },
                    grid: {
                        borderColor: '#f1f1f1',
                        xaxis: {
                            lines: { show: true }
                        }
                    },
                    series: response.series, // Dữ liệu từ server
                    xaxis: {
                        categories: response.categories, // Tên ngày trong tuần
                        labels: {
                            rotate: -45
                        }
                    },
                    yaxis: {
                        title: {
                            text: 'Số lượng bài'
                        },
                        opposite: isRtl
                    },
                    tooltip: {
                        y: {
                            formatter: function (val) {
                                return val + " bài";
                            }
                        }
                    },
                    fill: {
                        opacity: 1
                    }
                };

            if (columnChartEl) {
                columnChartEl.innerHTML = ''; // clear chart cũ
                var columnChart = new ApexCharts(columnChartEl, columnChartConfig);
                columnChart.render();
            }
        },
        error: function (xhr, status, error) {
            console.error('Lỗi lấy dữ liệu chart:', error);
        }
    });
}

function getDataStatisticTopTargetInWeek() {
    var isRtl = $('html').attr('data-textdirection') === 'rtl';

    $.ajax({
        url: '/Home/StatisticsTopActiveTargetInWeek', // Controller trả dữ liệu JSON
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            // Cấu hình chart
            var barChartEl = document.querySelector('#bar-chart'),
                barChartConfig = {
                    chart: {
                        height: 280,
                        type: 'bar',
                        parentHeightOffset: 0,
                        toolbar: {
                            show: false
                        }
                    },
                    plotOptions: {
                        bar: {
                            horizontal: true,
                            barHeight: '60%',
                            endingShape: 'rounded'
                        }
                    },
                    grid: {
                        xaxis: {
                            lines: {
                                show: false
                            }
                        },
                        padding: {
                            top: -15,
                            bottom: -10
                        }
                    },
                    colors: ['#12b0ff'], // hoặc window.colors.solid.info nếu có
                    dataLabels: {
                        enabled: false
                    },
                    series: [
                        {
                            name: 'Số lượng bài đăng',
                            data: response.series // số lượng bài đăng
                        }
                    ],
                    xaxis: {
                        categories: response.categories // tên các site
                    },
                    yaxis: {
                        opposite: isRtl
                    },
                    tooltip: {
                        y: {
                            formatter: function (val) {
                                return val + " bài";
                            }
                        }
                    }
                };

            if (typeof barChartEl !== undefined && barChartEl !== null) {
                barChartEl.innerHTML = ''; // clear chart cũ
                var barChart = new ApexCharts(barChartEl, barChartConfig);
                barChart.render();
            }
        },
        error: function (xhr, status, error) {
            console.error('Lỗi lấy dữ liệu Top Active Target:', error);
        }
    });
}


function loadTagCloud() {
    $.ajax({
        url: '/Home/TagCloudData', // hoặc controller phù hợp của bạn
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.error) {
                console.error(response.message);
                return;
            }

            const words = response.words;
            if (!words || words.length === 0) {
                $('#tagCloudContainer').html('<p class="text-center text-muted">No data available</p>');
                return;
            }

            // Chuyển sang định dạng list: [text, weight]
            const list = words.map(x => [x.text, x.size]);

            WordCloud(document.getElementById('tagCloudContainer'), {
                list: list,
                gridSize: 8,
                weightFactor: function (size) {
                    return size; // đã được scale sẵn từ controller
                },
                fontFamily: 'Arial, sans-serif',
                color: function () {
                    const colors = ['#1f77b4', '#ff7f0e', '#2ca02c', '#d62728',
                        '#9467bd', '#8c564b', '#e377c2', '#7f7f7f',
                        '#bcbd22', '#17becf'];
                    return colors[Math.floor(Math.random() * colors.length)];
                },
                rotateRatio: 0, // chỉ nằm ngang
                backgroundColor: '#fff',
                shuffle: true,
                drawOutOfBound: false,
                hover: function (item, dimension, event) {
                    if (item) {
                        event.target.title = item[0] + ' (' + item[1] + ')';
                    } else {
                        event.target.title = '';
                    }
                },
                click: function (item) {
                    alert(item[0] + ': ' + item[1]);
                }
            });
        },
        error: function (xhr, status, error) {
            console.error('AJAX error:', error);
        }
    });
}

function loadTopNewsSmooth() {
    $.ajax({
        url: '/Home/GetListNewsWeb',
        type: 'GET',
        success: function (res) {
            const data = res.data || res.items || [];
            if (data.length === 0) return;

            const container = $('#newsContainer');
            container.empty(); // Xóa hết tin cũ trước khi load mới

            let index = 0;

            function appendItem() {
                if (index >= data.length) {
                    // Khi đã hiển thị hết tin => chờ 3 giây rồi load lại
                    setTimeout(loadTopNewsSmooth, 3000);
                    return;
                }

                const item = data[index++];

                // Lấy giờ & ngày (định dạng HH:mm dd/MM)
                const time = (item.PostedDate?.substring(11, 16) || '') + ' ' +
                    (item.PostedDate?.substring(8, 10) || '') + '/' +
                    (item.PostedDate?.substring(5, 7) || '');

                // Xác định loại nguồn (blog / youtube / facebook)
                let icon = '';
                if (item.URL?.includes('youtube.com'))
                    icon = '<img src="https://cdn-icons-png.flaticon.com/512/1384/1384060.png" width="16" style="vertical-align:middle;">';
                else if (item.URL?.includes('facebook.com'))
                    icon = '<img src="https://cdn-icons-png.flaticon.com/512/1384/1384005.png" width="16" style="vertical-align:middle;">';
                else
                    icon = '<img src="https://cdn-icons-png.flaticon.com/512/535/535239.png" width="16" style="vertical-align:middle;">';

                // Tạo dòng tin
                const html = `
                    <div class="news-item" style="display:none; margin-bottom:6px;">
                        ${icon}
                        <span class="time" style="color:blue; font-weight:bold;"> ${time}</span>
                        <span class="source" style="color:red;"> (${item.SiteAddress || ''})</span>
                        <span class="title" style="font-weight:bold; color:black;"> ${item.Title || ''}</span><br>
                        <a href="${item.URL}" target="_blank" style="color:#0066cc; font-size:13px;">${item.URL}</a>
                    </div>
                `;

                // Thêm và hiển thị mượt
                const $el = $(html).appendTo(container);
                $el.slideDown(400, function () {
                    // Cuộn xuống dưới cùng để thấy tin mới
                    container.stop().animate({ scrollTop: container[0].scrollHeight }, 400);
                });

                // Tiếp tục thêm tin kế tiếp
                setTimeout(appendItem, 5000);
            }

            appendItem(); // Bắt đầu
        },
        error: function () {
            console.error("Không tải được danh sách tin.");
            // Thử lại sau 10 giây nếu lỗi
            setTimeout(loadTopNewsSmooth, 10000);
        }
    });
}