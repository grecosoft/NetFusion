/// <reference path="../../../angular.js" />

'use strict';

(function () {

    var module = angular.module('netfusion.structure');

    var compositeViewerController = ['$scope', 'CompositeLogService', function ($scope, compositeLogService) {

        var self = this;

        initViewModel();
        listenForLogUpdate();

       
        function initViewModel() {
            self.viewModel = {
                title: 'NetFusion-Composite Application',
                serverLog: null,
                hostLogs: [{ Name: "Waiting...", isDefault: true }]
            };

            compositeLogService.getCompositeLog().then(function (log) {
                self.viewModel.serverLog = log;
            });
        }

        function listenForLogUpdate() {
            var logHub = $.connection.compositeLogHub;

            logHub.client.log = function (hostLog) {

                var hostLogs = self.viewModel.hostLogs;
                if (hostLogs.length === 1 && hostLogs[0].isDefault) {
                    hostLogs = [];
                    self.viewModel.hostLogs = hostLogs;
                }

                hostLogs.push(hostLog);
                $scope.$apply();
            };

            $.connection.hub.start();
        }
    }];

    module.controller('CompositeViewerController', compositeViewerController);
})();