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
                hostPluginMenuItems: [],
                appComponentPluginMenuItems: [],
                corePLuginMenuItems: []
            };

            compositeLogService.getCompositeLog().then(function (hostLog) {

                var log = hostLog.Log;
                var compositeApp = hostLog.CompositeApp;

                self.viewModel.hostLogMenuItems.push(new HostLogMenuItem(
                    hostLog.HostName,
                    hostLog.HostPluginId));

                self.viewModel.serverLog = log;
                self.viewModel.title = hostLog.HostName;

                _hostLogs[hostLog.HostPluginId] = log;

                self.viewModel.hostPluginMenuItems = [new HostLogMenuItem(compositeApp.AppHostPlugin.Name, compositeApp.AppHostPlugin.PluginId)];
                self.viewModel.appComponentPluginMenuItems = createPluginList(compositeApp.AppComponentPlugins);
                self.viewModel.corePLuginMenuItems = createPluginList(compositeApp.CorePlugins);
                
                console.log(compositeApp);
            });
        }

        function createPluginList(plugins) {
            console.log(plugins);

            return _.map(plugins, function (p) {
                return new HostLogMenuItem(p.Name, p.PluginId)
            });
        }

        function listenForLogUpdate() {
            var logHub = $.connection.compositeLogHub;

            logHub.client.log = function (hostLog) {

                if (!_hostLogs[hostLog.HostPluginId]) {
                    self.viewModel.hostLogMenuItems.push(
                        new HostLogMenuItem(hostLog.HostName, hostLog.HostPluginId));
                }

                _hostLogs[hostLog.HostPluginId] = hostLog.Log;

                $scope.$apply();
            };

            $.connection.hub.start();
        }

        self.hostSelected = function(hostItem, ev) {
            self.viewModel.serverLog = _hostLogs[hostItem.pluginId];
            self.viewModel.title = hostItem.pluginName;
        }
    }];

    var HostLogMenuItem = function (name, pluginId) {
        this.pluginName = name;
        this.pluginId = pluginId;
    }

    module.controller('PortalController', portalController);
})();