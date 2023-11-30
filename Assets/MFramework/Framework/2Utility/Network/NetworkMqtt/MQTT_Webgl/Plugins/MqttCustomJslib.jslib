mergeInto(LibraryManager.library, {
  //Hello: function () {
  //  window.alert("测试Unity的Webgl平台通过H5调用MQTT通信");
  //},

  Jslib_Connect: function (host, port, clientId, username, password, destination) {
    mqttConnect(UTF8ToString(host), UTF8ToString(port), UTF8ToString(clientId), UTF8ToString(username), UTF8ToString(password), UTF8ToString(destination));
  },

  Jslib_Subscribe: function (topic) {
    mqttSubscribe(UTF8ToString(topic))
  },

  Jslib_Publish: function (topic, payload) {
    publish(UTF8ToString(topic), UTF8ToString(payload))
  },

  Jslib_Unsubscribe: function(topic) {
    mqttUnsubscribe(UTF8ToString(topic));
  },

  Jslib_Disconnect: function() {
    mqttDisconnect();
  },

 Jslib_RefreshWebgl: function() {
    refreshWebgl();
  }

});