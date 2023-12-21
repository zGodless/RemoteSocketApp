const watchControl = () => { // 监听事件
    timer = {};

    clientHeight = document.body.offsetHeight;
    clientWidth = document.body.offsetWidth;
    $('#img').onload = function () {
        // 获取 naturalWidth 属性
        var width = img.naturalWidth;
        console.log('图片 naturalWidth：' + width + 'px');
    };
    window.ondragstart = function (e) { // 移除拖动事件
        console.log('拖拽开始', e)
        e.preventDefault()
    }
    window.ondragend = function (e) { // 移除拖动事件
        console.log('拖拽结束', e)
        e.preventDefault()
    }
    window.onkeydown = function (e) { // 键盘按下
        console.log('键盘按下', e)
        //SendMsg(JSON.stringify({ type: 0, key: e.keyCode }))
    }
    window.onkeyup = function (e) { // 键盘抬起
        console.log('键盘抬起', e)
        //SendMsg(JSON.stringify({ type: 1, key: e.keyCode }))
    }
    window.onmousedown = function (e) { // 鼠标单击按下
        console.log('单击按下', e)
        const newPageX = parseInt(e.pageX * changeRateWidth) // 计算分辨率 = 当前坐标 * 缩放比例
        const newPageY = parseInt(e.pageY * changeRateHeight)
        console.log('newPageX', newPageX)
        console.log('newPageY', newPageY)
        SendMsg(JSON.stringify({ type: 5, x: newPageX, y: newPageY }))
    }
    window.onmouseup = function (e) { // 鼠标单击抬起
        console.log('单击抬起', e)
        const newPageX = parseInt(e.pageX * changeRateWidth) // 计算分辨率 = 当前坐标 * 缩放比例
        const newPageY = parseInt(e.pageY * changeRateHeight)
        console.log('newPageX', newPageX)
        console.log('newPageY', newPageY)
        SendMsg(JSON.stringify({ type: 6, x: newPageX, y: newPageY }))
    }
    window.oncontextmenu = function (e) { // 鼠标右击
        console.log('右击', e)
        e.preventDefault()
        const newPageX = parseInt(e.pageX * changeRateWidth) // 计算分辨率 = 当前坐标 * 缩放比例
        const newPageY = parseInt(e.pageY * changeRateHeight)
        console.log('newPageX', newPageX)
        console.log('newPageY', newPageY)
        SendMsg(JSON.stringify({ type: 4, x: newPageX, y: newPageY }))
    }
    window.ondblclick = function (e) { // 鼠标双击
        console.log('双击', e)
    }
    window.onmousewheel = function (e) { // 鼠标滚动
        console.log('滚动', e)
        const moving = e.deltaY / e.wheelDeltaY
        //SendMsg(JSON.stringify({ type: 7, x: e.x, y: e.y, deltaY: e.deltaY, deltaFactor: moving }))
    }
    window.onmousemove = function (e) { // 鼠标移动

        const newPageX = parseInt(e.pageX * changeRateWidth) // 计算分辨率 = 当前坐标 * 缩放比例
        const newPageY = parseInt(e.pageY * changeRateHeight)
        //console.log("鼠标移动:X轴位置" + newPageX + ";Y轴位置:" + newPageY)
        SendMsg(JSON.stringify({ type: 2, x: newPageX, y: newPageY }))
    }
}
function SendMsg(content) {
    if (webSocket != undefined && !!webSocket) {
        webSocket.send(content);
    }
}