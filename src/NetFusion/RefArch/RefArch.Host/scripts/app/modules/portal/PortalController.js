/// <reference path="../../../angular.js" />

'use strict';

(function () {

    var module = angular.module('netfusion.structure');

    var portalController = ['$scope', '$mdMenu', 'CompositeLogService',
        function ($scope, $mdMenu, compositeLogService) {

        var self = this;

        var _hostLogs = [];

        initMenuItems();
        initViewModel();
        listenForLogUpdate();
       
        function initViewModel() {
            self.viewModel = {
                title: 'NetFusion-Composite Application',
                serverLog: null,
                hostLogMenuItems: [],
            };

            compositeLogService.getCompositeLog().then(function (log) {
                self.viewModel.hostLogMenuItems.push(new MenuItem(log.Name, null));
                self.viewModel.serverLog = log;

                _hostLogs.push(log);
            });
        }

        function initMenuItems() {
            self.menu = [
                new MenuItem("Clients", null),
                new MenuItem("Plug-Ins", null),
                new MenuItem("API", null),
                new MenuItem("Logs", null)
            ];
        }

        function listenForLogUpdate() {
            var logHub = $.connection.compositeLogHub;

            logHub.client.log = function (hostLog) {

                _hostLogs.push(hostLog);
                self.viewModel.hostLogMenuItems.push(new MenuItem(hostLog.Name, null));
                $scope.$apply();
            };

            $.connection.hub.start();
        }
    }];

    var MenuItem = function (title, factory) {
        this.title = title;
        this.factory = factory;
        this.items = [];

        if (factory && angular.isFunction(factory)) {
            factory.call(this)
        }
    }

    module.controller('PortalController', portalController);
})();