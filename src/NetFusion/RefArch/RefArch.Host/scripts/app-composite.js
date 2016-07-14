/// <reference path="angular.js" />
/// <reference path="app.js" />

'use strict';

(function () {

	var module = angular.module('netfusion.app.composite', ['jsonFormatter']);

	var compositeViewerController = ['$scope', 'compositeLogService', function ($scope, compositeLogService) {

	    compositeLogService.getCompositeLog().then(function (log) {
	        $scope.log = log;
	    });

	    var listenForLogUpdate = function () {
	        var logHub = $.connection.compositeLogHub;
	        // Create a function that the hub can call to broadcast messages.
	        logHub.client.log = function (log) {
	            $scope.log = log;
	            $scope.$apply();
	        };

	        $.connection.hub.start();
	    }

	    listenForLogUpdate();
	}];

	module.controller('compositeViewerController',
        compositeViewerController);
    

	var compositeLogService = ['$http', function ($http) {

	    this.getCompositeLog = function() {
	        return $http({
	            method: 'GET',
	            url: 'api/config/composite/log'
	        }).then(function (response) {
	            return response.data;
	        });
	    };

	}];

	module.service('compositeLogService',
        compositeLogService);

}());