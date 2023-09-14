mergeInto(LibraryManager.library, {
  Hello: function () {
    window.alert("测试Unity的Webgl平台通过H5调用MQTT通信");
  },
  
  HelloString: function (str) {
    // window.alert(Pointer_stringify(str));
    window.alert(UTF8ToString(str));
  },

  PrintFloatArray: function (array, size) {
    for(var i = 0; i < size; i++)
    console.log(HEAPF32[(array >> 2) + i]);
  },

  AddNumbers: function (x, y) {
    return x + y;
  },

  StringReturnValueFunction: function () {
    var returnStr = "bla";
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  BindWebGLTexture: function (texture) {
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texture]);
  },


  Jslib_Connect: function (host, port, clientId, username, password, destination) {
    mqttConnect(UTF8ToString(host), UTF8ToString(port), UTF8ToString(clientId), UTF8ToString(username), UTF8ToString(password), UTF8ToString(destination));
  },

  Jslib_Subscribe: function (topic) {
    mqttSubscribe(UTF8ToString(topic))
  },

  Jslib_Publish: function (topic, payload) {
    mqttSend(UTF8ToString(topic), UTF8ToString(payload))
  },

  Jslib_Unsubscribe: function(topic) {
    mqttUnsubscribe(UTF8ToString(topic));
  },

  Jslib_Disconnect: function() {
    mqttDisconnect();
  }
});