/// <reference path="angular.js" />
/// <reference path="app.js" />

'use strict';

(function () {

	var module = angular.module('netfusion.app.composite', ['jsonFormatter']);

	var compositeViewerController = ['$scope', 'compositeLogService', function ($scope, compositeLogService) {

	    compositeLogService.getCompositeLog().then(function (log) {
	        $scope.log = log;
	    })
	}];

	module.controller('compositeViewerController',
        compositeViewerController);
    

	var compositeLogService = ['$http', function ($http) {

	    this.getCompositeLog = function() {
	        return $http({
	            method: 'GET',
	            url: 'api/samples/web/composite/log'
	        }).then(function (response) {
	            return response.data;
	        });;
		}

	}];

	module.service('compositeLogService',
        compositeLogService);

}());