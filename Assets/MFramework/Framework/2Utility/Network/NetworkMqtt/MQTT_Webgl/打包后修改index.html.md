### **只需在Unity打包Webgl后的index.html中<body>标签中增加以下代码，即可完成unityWebgl端基于mqtt协议通信**



#### Add Scripts1

```js
  <script src="https://unpkg.com/mqtt/dist/mqtt.min.js"></script>
  <script>
    //#region MQTT 连接、断开、订阅监听消息、发送消息
    /*
    MQTT类库https://unpkg.com/mqtt/dist/mqtt.min.js  
    API文档：https://www.emqx.com/zh/blog/mqtt-js-tutorial
    */
    var client
    var gameUnityInstance
    var subscribeIdObj = {}
    const options = {
      Name: 'Name_Test_MQTT_WebGl',
      connectTimeout: 40000,
      clientId: '',
      username: '',
      password: '',
      cleanSession: false,
      keepAlive: 60
    }

    //ArrayBuffer二进制转字符串
    function ab2str(message) {
      const utf8decoder = new TextDecoder()
      if (!message?.length) return null
      const type = Object.prototype.toString.call(message)
      let res = null
      if (type === '[object Uint8Array]') {
        // @ts-ignore
        res = utf8decoder.decode(new Uint8Array(message))
      } else {
        res = JSON.stringify(message)
      }
      if (Object.prototype.toString.call(res) !== '[object String]') {
        res = JSON.parse(res)
      }
      return res
    }

    //连接
    function mqttConnect(host, port, clientId, username, password, destination) {
      let url = 'ws://' + host + ':' + port + '/mqtt'
      //创建一个client实例
      options.clientId = clientId
      options.username = username
      options.password = password

      //Test
      url = 'ws://10.5.24.27:8083/mqtt'

      client = mqtt.connect(url, options)
      log("尝试链接mqtt IP:" + host + ",port:" + port + ",username:" + options.username + "clientId: " + options.clientId);

      var firstConnSuc = false;
      client.on('connect', function (connack) {
        if (!firstConnSuc) {
          firstConnSuc = true
          log("mqtt首次连接成功! username :" + options.username + "clientId: " + options.clientId, true);
          gameUnityInstance.SendMessage('[UnityObjectForWebglMsg]', 'ConnSuc')
        }
      })
    }

    //订阅监听消息
    function mqttSubscribe(topic) {
      log("尝试订阅监听消息 topic:" + topic);

      //topic: 可传入一个字符串，或者一个字符串数组，也可以是一个 topic 对象，{'test1': {qos: 0}, 'test2': {qos: 1}}
      //options: 可选值，订阅 Topic 时的配置信息，主要是填写订阅的 Topic 的 QoS 等级的
      //callback: 订阅 Topic 后的回调函数，参数为 error 和 granted，当订阅失败时 error 参数才存在, granted 是一个 { topic, qos } 的数组，其中 topic 是一个被订阅的主题，qos 是 Topic 是被授予的 QoS 等级
      subscribeIdObj[topic] = client.subscribe(topic, { qos: 0 }, function (error, granted) {
        if (error) {
          error("订阅监听消息失败 error:" + error)
        } else {
          if (granted.length > 0) {
            log(`订阅监听消息成功 topic: ${granted[0].topic}`)
          }
          else {
            error("订阅监听消息失败,当前标题已订阅 topic:" + topic)
          }
        }
      })

      client.on('message', function (_topic, message) {
        //二进制ArrayBuffer消息转字符串
        var msgStr = ab2str(message)
        log("接收到消息,msg:" + msgStr);
        gameUnityInstance.SendMessage('[UnityObjectForWebglMsg]', 'RecvMsg', _topic + "|" + message)
      })
    }

    //发送消息
    function publish(topic, payload) {
      log("尝试发送消息 , topic:" + topic + ", msg:" + payload);
      // 发布消息
      client.publish(topic, payload, { qos: 0, retain: false }, function (error) {
        if (error) {
          error("发送消息失败 error:" + error + ',topic:' + topic + ',msg:' + payload)
        } else {
          log('发送消息成功 topic:' + topic + ',msg:' + payload)
        }
      })
    }

    //取消消息订阅
    function mqttUnsubscribe(topic) {
      log('尝试取消消息订阅  topic:' + topic)
      client.unsubscribe(topic, function (error) {
        if (error) {
          log('取消消息订阅失败 topic:' + topic)
        } else {
          log('取消消息订阅成功 topic:' + topic)
        }
      })
    }

    // 断开连接
    function mqttDisconnect() {
      log("尝试断开mqtt链接");
      client.end(true, null, () => {
        log('已断开mqtt链接')
      })
    }
    //封装js日志打印
    function log(msg, isShow = true) {
      if (isShow) {
        console.log("[html]:" + msg);
      }
    }
    function error(msg, isShow = true) {
      if (isShow) {
        console.error("[html]:" + msg);
      }
    }
    //#endregion MQTT
  </script>
```



#### Add Scripts2

找到此代码段，只新增一行代码

```js
var script = document.createElement("script");
    script.src = loaderUrl;
    script.onload = () => {
      createUnityInstance(canvas, config, (progress) => {
        progressBarFull.style.width = 100 * progress + "%";
      }).then((unityInstance) => {
        gameUnityInstance = unityInstance    //当前代码端仅仅新增此行，当前代码端仅仅新增此行，当前代码端仅仅新增此行
        loadingBar.style.display = "none";
        fullscreenButton.onclick = () => {
          unityInstance.SetFullscreen(1);
        };
      }).catch((message) => {
        alert(message);
      });
    };
    document.body.appendChild(script);
```

