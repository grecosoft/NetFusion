/// <reference path="../../../angular.js" />

'use strict';

(function () {

    var module = angular.module('netfusion.structure');

    var portalController = ['$scope', 'CompositeLogService',
        function ($scope, compositeLogService) {

        var self = this;

        var _hostLogs = {};

        initViewModel();
        listenForLogUpdate();
       
        function initViewModel() {
            self.viewModel = {
                title: 'NetFusion-Composite Application',
                serverLog: null,
                hostLogMenuItems: [],
                hostPluginMenuItems: []
            };

            compositeLogService.getCompositeLog().then(function (hostLog) {

                var log = hostLog.Log;

                self.viewModel.hostLogMenuItems.push(new HostLogMenuItem(
                    hostLog.HostName,
                    hostLog.HostPluginId));

                self.viewModel.serverLog = log;
                self.viewModel.title = hostLog.HostName;

                _hostLogs[hostLog.HostPluginId] = log;

                console.log(hostLog.CompositeApp);
            });
        }

        function listenForLogUpdate() {
            var logHub = $.connection.compositeLogHub;

            logHub.client.log = function (hostLog) {

                if (!_hostLogs[hostLog.HostPluginId]) {
                    self.viewModel.hostLogMenuItems.push(
                        new HostLogMenuItem(hostLog.HostName, hostLog.HostPluginId));
                }

                _hostLogs[hostLog.HostPluginId] = hostLog;

                $scope.$apply();
            };

            $.connection.hub.start();
        }

        self.hostSelected = function(hostItem, ev) {
            self.viewModel.serverLog = _hostLogs[hostItem.hostPluginId];
            self.viewModel.title = hostItem.hostName;
        }
    }];

    var HostLogMenuItem = function (name, pluginId) {
        this.hostName = name;
        this.hostPluginId = pluginId;
    }

    module.controller('PortalController', portalController);
})();