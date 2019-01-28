// https://github.com/temich666/t-json-viewer

/* tslint:disable */

import {Component, Input, Output, EventEmitter, ViewEncapsulation, OnChanges} from '@angular/core';

interface Item {
  key: string;
  value: any;
  title: string;
  type: string;
  isOpened: boolean;
}

@Component({
  selector: 'json-viewer',
  templateUrl: './json-viewer.component.html',
  styleUrls: ['./json-viewer.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class JsonViewerComponent implements OnChanges {

  @Input()
  public json: Array<any> | Object | any;

  @Input()
  public showNodeNav: boolean = false;

  @Output()
  public itemSelected = new EventEmitter<any>();


  public asset: Array<Item> = [];

  constructor() { }

  ngOnChanges() {
    // Do nothing without data
    if (typeof (this.json) !== 'object' && !Array.isArray(this.json)) {
      return;
    }

    // Make the asset array empty again
    this.asset = [];

    /**
     * Convert json to array of items
     */
    Object.keys(this.json).forEach((key) => {
      this.asset.push(this.createItem(key, this.json[key]));
    });
  }

  /**
   * Check value and Create item object
   * @param {string|any} key
   * @param {any} value
   */
  private createItem(key: any, value: any): Item {
    let item: Item = {
      key: key || '""', // original key or empty string
      value: value, // original value
      title: value, // title by default
      type: undefined,
      isOpened: false // closed by default
    };

    if (typeof (item.value) === 'string') {
      item.type = 'string';
      item.title = `"${item.value}"`;
    } else if (typeof (item.value) === 'number') {
      item.type = 'number';
    } else if (typeof (item.value) === 'boolean') {
      item.type = 'boolean';
    } else if (item.value instanceof Date) {
      item.type = 'date';
    } else if (typeof (item.value) === 'function') {
      item.type = 'function';
    } else if (Array.isArray(item.value)) {
      item.type = 'array';
      item.title = `Array[${item.value.length}]`;
    } else if (item.value === null) {
      item.type = 'null';
      item.title = 'null'
    } else if (typeof (item.value) === 'object') {
      item.type = 'object';
      item.title = `Object ${"{}"}`;
    } else if (item.value === undefined) {
      item.type = 'undefined';
      item.title = 'undefined'
    }

    item.title = '' + item.title; // defined type or 'undefined'

    return item;
  }

  private getObjectSummary(obj: any): string {
    let propNames = Object.getOwnPropertyNames(obj).slice(0, 2);
    let summaryObj = {};

    for (let i=0; i<propNames.length; i++) {
      summaryObj[propNames[i]] = obj[propNames[i]];
    }

    return JSON.stringify(summaryObj);
  }

  /**
   * Check item's type for Array or Object
   * @param {Item} item
   * @return {boolean}
   */
  isObject(item: Item): boolean {
    return ['object', 'array'].indexOf(item.type) !== -1;
  }

  /**
   * Handle click event on collapsable item
   * @param {Item} item
   */
  clickHandle(item: Item) {
    if (!this.isObject(item)) {
      return;
    }
    item.isOpened = !item.isOpened;
  }

  public nodeSelected(item: Item) {
    this.itemSelected.emit(item.value);
  }

}
