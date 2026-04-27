/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ "./node_modules/@floating-ui/dom/dist/floating-ui.dom.esm.js":
/*!*******************************************************************!*\
  !*** ./node_modules/@floating-ui/dom/dist/floating-ui.dom.esm.js ***!
  \*******************************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "arrow": () => (/* binding */ arrow),
/* harmony export */   "autoPlacement": () => (/* binding */ autoPlacement),
/* harmony export */   "autoUpdate": () => (/* binding */ autoUpdate),
/* harmony export */   "computePosition": () => (/* binding */ computePosition),
/* harmony export */   "detectOverflow": () => (/* binding */ detectOverflow),
/* harmony export */   "flip": () => (/* binding */ flip),
/* harmony export */   "getOverflowAncestors": () => (/* reexport safe */ _floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getOverflowAncestors),
/* harmony export */   "hide": () => (/* binding */ hide),
/* harmony export */   "inline": () => (/* binding */ inline),
/* harmony export */   "limitShift": () => (/* binding */ limitShift),
/* harmony export */   "offset": () => (/* binding */ offset),
/* harmony export */   "platform": () => (/* binding */ platform),
/* harmony export */   "shift": () => (/* binding */ shift),
/* harmony export */   "size": () => (/* binding */ size)
/* harmony export */ });
/* harmony import */ var _floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @floating-ui/utils */ "./node_modules/@floating-ui/utils/dist/floating-ui.utils.mjs");
/* harmony import */ var _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @floating-ui/core */ "./node_modules/@floating-ui/core/dist/floating-ui.core.mjs");
/* harmony import */ var _floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @floating-ui/utils/dom */ "./node_modules/@floating-ui/utils/dist/floating-ui.utils.dom.mjs");





function getCssDimensions(element) {
  const css = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(element);
  // In testing environments, the `width` and `height` properties are empty
  // strings for SVG elements, returning NaN. Fallback to `0` in this case.
  let width = parseFloat(css.width) || 0;
  let height = parseFloat(css.height) || 0;
  const hasOffset = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isHTMLElement)(element);
  const offsetWidth = hasOffset ? element.offsetWidth : width;
  const offsetHeight = hasOffset ? element.offsetHeight : height;
  const shouldFallback = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.round)(width) !== offsetWidth || (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.round)(height) !== offsetHeight;
  if (shouldFallback) {
    width = offsetWidth;
    height = offsetHeight;
  }
  return {
    width,
    height,
    $: shouldFallback
  };
}

function unwrapElement(element) {
  return !(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement)(element) ? element.contextElement : element;
}

function getScale(element) {
  const domElement = unwrapElement(element);
  if (!(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isHTMLElement)(domElement)) {
    return (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(1);
  }
  const rect = domElement.getBoundingClientRect();
  const {
    width,
    height,
    $
  } = getCssDimensions(domElement);
  let x = ($ ? (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.round)(rect.width) : rect.width) / width;
  let y = ($ ? (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.round)(rect.height) : rect.height) / height;

  // 0, NaN, or Infinity should always fallback to 1.

  if (!x || !Number.isFinite(x)) {
    x = 1;
  }
  if (!y || !Number.isFinite(y)) {
    y = 1;
  }
  return {
    x,
    y
  };
}

const noOffsets = /*#__PURE__*/(0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(0);
function getVisualOffsets(element) {
  const win = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getWindow)(element);
  if (!(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isWebKit)() || !win.visualViewport) {
    return noOffsets;
  }
  return {
    x: win.visualViewport.offsetLeft,
    y: win.visualViewport.offsetTop
  };
}
function shouldAddVisualOffsets(element, isFixed, floatingOffsetParent) {
  if (isFixed === void 0) {
    isFixed = false;
  }
  if (!floatingOffsetParent || isFixed && floatingOffsetParent !== (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getWindow)(element)) {
    return false;
  }
  return isFixed;
}

function getBoundingClientRect(element, includeScale, isFixedStrategy, offsetParent) {
  if (includeScale === void 0) {
    includeScale = false;
  }
  if (isFixedStrategy === void 0) {
    isFixedStrategy = false;
  }
  const clientRect = element.getBoundingClientRect();
  const domElement = unwrapElement(element);
  let scale = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(1);
  if (includeScale) {
    if (offsetParent) {
      if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement)(offsetParent)) {
        scale = getScale(offsetParent);
      }
    } else {
      scale = getScale(element);
    }
  }
  const visualOffsets = shouldAddVisualOffsets(domElement, isFixedStrategy, offsetParent) ? getVisualOffsets(domElement) : (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(0);
  let x = (clientRect.left + visualOffsets.x) / scale.x;
  let y = (clientRect.top + visualOffsets.y) / scale.y;
  let width = clientRect.width / scale.x;
  let height = clientRect.height / scale.y;
  if (domElement) {
    const win = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getWindow)(domElement);
    const offsetWin = offsetParent && (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement)(offsetParent) ? (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getWindow)(offsetParent) : offsetParent;
    let currentWin = win;
    let currentIFrame = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getFrameElement)(currentWin);
    while (currentIFrame && offsetParent && offsetWin !== currentWin) {
      const iframeScale = getScale(currentIFrame);
      const iframeRect = currentIFrame.getBoundingClientRect();
      const css = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(currentIFrame);
      const left = iframeRect.left + (currentIFrame.clientLeft + parseFloat(css.paddingLeft)) * iframeScale.x;
      const top = iframeRect.top + (currentIFrame.clientTop + parseFloat(css.paddingTop)) * iframeScale.y;
      x *= iframeScale.x;
      y *= iframeScale.y;
      width *= iframeScale.x;
      height *= iframeScale.y;
      x += left;
      y += top;
      currentWin = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getWindow)(currentIFrame);
      currentIFrame = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getFrameElement)(currentWin);
    }
  }
  return (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.rectToClientRect)({
    width,
    height,
    x,
    y
  });
}

// If <html> has a CSS width greater than the viewport, then this will be
// incorrect for RTL.
function getWindowScrollBarX(element, rect) {
  const leftScroll = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getNodeScroll)(element).scrollLeft;
  if (!rect) {
    return getBoundingClientRect((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement)(element)).left + leftScroll;
  }
  return rect.left + leftScroll;
}

function getHTMLOffset(documentElement, scroll, ignoreScrollbarX) {
  if (ignoreScrollbarX === void 0) {
    ignoreScrollbarX = false;
  }
  const htmlRect = documentElement.getBoundingClientRect();
  const x = htmlRect.left + scroll.scrollLeft - (ignoreScrollbarX ? 0 :
  // RTL <body> scrollbar.
  getWindowScrollBarX(documentElement, htmlRect));
  const y = htmlRect.top + scroll.scrollTop;
  return {
    x,
    y
  };
}

function convertOffsetParentRelativeRectToViewportRelativeRect(_ref) {
  let {
    elements,
    rect,
    offsetParent,
    strategy
  } = _ref;
  const isFixed = strategy === 'fixed';
  const documentElement = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement)(offsetParent);
  const topLayer = elements ? (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isTopLayer)(elements.floating) : false;
  if (offsetParent === documentElement || topLayer && isFixed) {
    return rect;
  }
  let scroll = {
    scrollLeft: 0,
    scrollTop: 0
  };
  let scale = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(1);
  const offsets = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(0);
  const isOffsetParentAnElement = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isHTMLElement)(offsetParent);
  if (isOffsetParentAnElement || !isOffsetParentAnElement && !isFixed) {
    if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getNodeName)(offsetParent) !== 'body' || (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isOverflowElement)(documentElement)) {
      scroll = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getNodeScroll)(offsetParent);
    }
    if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isHTMLElement)(offsetParent)) {
      const offsetRect = getBoundingClientRect(offsetParent);
      scale = getScale(offsetParent);
      offsets.x = offsetRect.x + offsetParent.clientLeft;
      offsets.y = offsetRect.y + offsetParent.clientTop;
    }
  }
  const htmlOffset = documentElement && !isOffsetParentAnElement && !isFixed ? getHTMLOffset(documentElement, scroll, true) : (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(0);
  return {
    width: rect.width * scale.x,
    height: rect.height * scale.y,
    x: rect.x * scale.x - scroll.scrollLeft * scale.x + offsets.x + htmlOffset.x,
    y: rect.y * scale.y - scroll.scrollTop * scale.y + offsets.y + htmlOffset.y
  };
}

function getClientRects(element) {
  return Array.from(element.getClientRects());
}

// Gets the entire size of the scrollable document area, even extending outside
// of the `<html>` and `<body>` rect bounds if horizontally scrollable.
function getDocumentRect(element) {
  const html = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement)(element);
  const scroll = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getNodeScroll)(element);
  const body = element.ownerDocument.body;
  const width = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.max)(html.scrollWidth, html.clientWidth, body.scrollWidth, body.clientWidth);
  const height = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.max)(html.scrollHeight, html.clientHeight, body.scrollHeight, body.clientHeight);
  let x = -scroll.scrollLeft + getWindowScrollBarX(element);
  const y = -scroll.scrollTop;
  if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(body).direction === 'rtl') {
    x += (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.max)(html.clientWidth, body.clientWidth) - width;
  }
  return {
    width,
    height,
    x,
    y
  };
}

function getViewportRect(element, strategy) {
  const win = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getWindow)(element);
  const html = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement)(element);
  const visualViewport = win.visualViewport;
  let width = html.clientWidth;
  let height = html.clientHeight;
  let x = 0;
  let y = 0;
  if (visualViewport) {
    width = visualViewport.width;
    height = visualViewport.height;
    const visualViewportBased = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isWebKit)();
    if (!visualViewportBased || visualViewportBased && strategy === 'fixed') {
      x = visualViewport.offsetLeft;
      y = visualViewport.offsetTop;
    }
  }
  return {
    width,
    height,
    x,
    y
  };
}

// Returns the inner client rect, subtracting scrollbars if present.
function getInnerBoundingClientRect(element, strategy) {
  const clientRect = getBoundingClientRect(element, true, strategy === 'fixed');
  const top = clientRect.top + element.clientTop;
  const left = clientRect.left + element.clientLeft;
  const scale = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isHTMLElement)(element) ? getScale(element) : (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(1);
  const width = element.clientWidth * scale.x;
  const height = element.clientHeight * scale.y;
  const x = left * scale.x;
  const y = top * scale.y;
  return {
    width,
    height,
    x,
    y
  };
}
function getClientRectFromClippingAncestor(element, clippingAncestor, strategy) {
  let rect;
  if (clippingAncestor === 'viewport') {
    rect = getViewportRect(element, strategy);
  } else if (clippingAncestor === 'document') {
    rect = getDocumentRect((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement)(element));
  } else if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement)(clippingAncestor)) {
    rect = getInnerBoundingClientRect(clippingAncestor, strategy);
  } else {
    const visualOffsets = getVisualOffsets(element);
    rect = {
      x: clippingAncestor.x - visualOffsets.x,
      y: clippingAncestor.y - visualOffsets.y,
      width: clippingAncestor.width,
      height: clippingAncestor.height
    };
  }
  return (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.rectToClientRect)(rect);
}
function hasFixedPositionAncestor(element, stopNode) {
  const parentNode = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getParentNode)(element);
  if (parentNode === stopNode || !(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement)(parentNode) || (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isLastTraversableNode)(parentNode)) {
    return false;
  }
  return (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(parentNode).position === 'fixed' || hasFixedPositionAncestor(parentNode, stopNode);
}

// A "clipping ancestor" is an `overflow` element with the characteristic of
// clipping (or hiding) child elements. This returns all clipping ancestors
// of the given element up the tree.
function getClippingElementAncestors(element, cache) {
  const cachedResult = cache.get(element);
  if (cachedResult) {
    return cachedResult;
  }
  let result = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getOverflowAncestors)(element, [], false).filter(el => (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement)(el) && (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getNodeName)(el) !== 'body');
  let currentContainingBlockComputedStyle = null;
  const elementIsFixed = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(element).position === 'fixed';
  let currentNode = elementIsFixed ? (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getParentNode)(element) : element;

  // https://developer.mozilla.org/en-US/docs/Web/CSS/Containing_block#identifying_the_containing_block
  while ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement)(currentNode) && !(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isLastTraversableNode)(currentNode)) {
    const computedStyle = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(currentNode);
    const currentNodeIsContaining = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isContainingBlock)(currentNode);
    if (!currentNodeIsContaining && computedStyle.position === 'fixed') {
      currentContainingBlockComputedStyle = null;
    }
    const shouldDropCurrentNode = elementIsFixed ? !currentNodeIsContaining && !currentContainingBlockComputedStyle : !currentNodeIsContaining && computedStyle.position === 'static' && !!currentContainingBlockComputedStyle && ['absolute', 'fixed'].includes(currentContainingBlockComputedStyle.position) || (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isOverflowElement)(currentNode) && !currentNodeIsContaining && hasFixedPositionAncestor(element, currentNode);
    if (shouldDropCurrentNode) {
      // Drop non-containing blocks.
      result = result.filter(ancestor => ancestor !== currentNode);
    } else {
      // Record last containing block for next iteration.
      currentContainingBlockComputedStyle = computedStyle;
    }
    currentNode = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getParentNode)(currentNode);
  }
  cache.set(element, result);
  return result;
}

// Gets the maximum area that the element is visible in due to any number of
// clipping ancestors.
function getClippingRect(_ref) {
  let {
    element,
    boundary,
    rootBoundary,
    strategy
  } = _ref;
  const elementClippingAncestors = boundary === 'clippingAncestors' ? (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isTopLayer)(element) ? [] : getClippingElementAncestors(element, this._c) : [].concat(boundary);
  const clippingAncestors = [...elementClippingAncestors, rootBoundary];
  const firstClippingAncestor = clippingAncestors[0];
  const clippingRect = clippingAncestors.reduce((accRect, clippingAncestor) => {
    const rect = getClientRectFromClippingAncestor(element, clippingAncestor, strategy);
    accRect.top = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.max)(rect.top, accRect.top);
    accRect.right = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.min)(rect.right, accRect.right);
    accRect.bottom = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.min)(rect.bottom, accRect.bottom);
    accRect.left = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.max)(rect.left, accRect.left);
    return accRect;
  }, getClientRectFromClippingAncestor(element, firstClippingAncestor, strategy));
  return {
    width: clippingRect.right - clippingRect.left,
    height: clippingRect.bottom - clippingRect.top,
    x: clippingRect.left,
    y: clippingRect.top
  };
}

function getDimensions(element) {
  const {
    width,
    height
  } = getCssDimensions(element);
  return {
    width,
    height
  };
}

function getRectRelativeToOffsetParent(element, offsetParent, strategy) {
  const isOffsetParentAnElement = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isHTMLElement)(offsetParent);
  const documentElement = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement)(offsetParent);
  const isFixed = strategy === 'fixed';
  const rect = getBoundingClientRect(element, true, isFixed, offsetParent);
  let scroll = {
    scrollLeft: 0,
    scrollTop: 0
  };
  const offsets = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(0);
  if (isOffsetParentAnElement || !isOffsetParentAnElement && !isFixed) {
    if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getNodeName)(offsetParent) !== 'body' || (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isOverflowElement)(documentElement)) {
      scroll = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getNodeScroll)(offsetParent);
    }
    if (isOffsetParentAnElement) {
      const offsetRect = getBoundingClientRect(offsetParent, true, isFixed, offsetParent);
      offsets.x = offsetRect.x + offsetParent.clientLeft;
      offsets.y = offsetRect.y + offsetParent.clientTop;
    } else if (documentElement) {
      // If the <body> scrollbar appears on the left (e.g. RTL systems). Use
      // Firefox with layout.scrollbar.side = 3 in about:config to test this.
      offsets.x = getWindowScrollBarX(documentElement);
    }
  }
  const htmlOffset = documentElement && !isOffsetParentAnElement && !isFixed ? getHTMLOffset(documentElement, scroll) : (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.createCoords)(0);
  const x = rect.left + scroll.scrollLeft - offsets.x - htmlOffset.x;
  const y = rect.top + scroll.scrollTop - offsets.y - htmlOffset.y;
  return {
    x,
    y,
    width: rect.width,
    height: rect.height
  };
}

function isStaticPositioned(element) {
  return (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(element).position === 'static';
}

function getTrueOffsetParent(element, polyfill) {
  if (!(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isHTMLElement)(element) || (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(element).position === 'fixed') {
    return null;
  }
  if (polyfill) {
    return polyfill(element);
  }
  let rawOffsetParent = element.offsetParent;

  // Firefox returns the <html> element as the offsetParent if it's non-static,
  // while Chrome and Safari return the <body> element. The <body> element must
  // be used to perform the correct calculations even if the <html> element is
  // non-static.
  if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement)(element) === rawOffsetParent) {
    rawOffsetParent = rawOffsetParent.ownerDocument.body;
  }
  return rawOffsetParent;
}

// Gets the closest ancestor positioned element. Handles some edge cases,
// such as table ancestors and cross browser bugs.
function getOffsetParent(element, polyfill) {
  const win = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getWindow)(element);
  if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isTopLayer)(element)) {
    return win;
  }
  if (!(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isHTMLElement)(element)) {
    let svgOffsetParent = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getParentNode)(element);
    while (svgOffsetParent && !(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isLastTraversableNode)(svgOffsetParent)) {
      if ((0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement)(svgOffsetParent) && !isStaticPositioned(svgOffsetParent)) {
        return svgOffsetParent;
      }
      svgOffsetParent = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getParentNode)(svgOffsetParent);
    }
    return win;
  }
  let offsetParent = getTrueOffsetParent(element, polyfill);
  while (offsetParent && (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isTableElement)(offsetParent) && isStaticPositioned(offsetParent)) {
    offsetParent = getTrueOffsetParent(offsetParent, polyfill);
  }
  if (offsetParent && (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isLastTraversableNode)(offsetParent) && isStaticPositioned(offsetParent) && !(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isContainingBlock)(offsetParent)) {
    return win;
  }
  return offsetParent || (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getContainingBlock)(element) || win;
}

const getElementRects = async function (data) {
  const getOffsetParentFn = this.getOffsetParent || getOffsetParent;
  const getDimensionsFn = this.getDimensions;
  const floatingDimensions = await getDimensionsFn(data.floating);
  return {
    reference: getRectRelativeToOffsetParent(data.reference, await getOffsetParentFn(data.floating), data.strategy),
    floating: {
      x: 0,
      y: 0,
      width: floatingDimensions.width,
      height: floatingDimensions.height
    }
  };
};

function isRTL(element) {
  return (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getComputedStyle)(element).direction === 'rtl';
}

const platform = {
  convertOffsetParentRelativeRectToViewportRelativeRect,
  getDocumentElement: _floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement,
  getClippingRect,
  getOffsetParent,
  getElementRects,
  getClientRects,
  getDimensions,
  getScale,
  isElement: _floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.isElement,
  isRTL
};

// https://samthor.au/2021/observing-dom/
function observeMove(element, onMove) {
  let io = null;
  let timeoutId;
  const root = (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getDocumentElement)(element);
  function cleanup() {
    var _io;
    clearTimeout(timeoutId);
    (_io = io) == null || _io.disconnect();
    io = null;
  }
  function refresh(skip, threshold) {
    if (skip === void 0) {
      skip = false;
    }
    if (threshold === void 0) {
      threshold = 1;
    }
    cleanup();
    const {
      left,
      top,
      width,
      height
    } = element.getBoundingClientRect();
    if (!skip) {
      onMove();
    }
    if (!width || !height) {
      return;
    }
    const insetTop = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.floor)(top);
    const insetRight = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.floor)(root.clientWidth - (left + width));
    const insetBottom = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.floor)(root.clientHeight - (top + height));
    const insetLeft = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.floor)(left);
    const rootMargin = -insetTop + "px " + -insetRight + "px " + -insetBottom + "px " + -insetLeft + "px";
    const options = {
      rootMargin,
      threshold: (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.max)(0, (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_1__.min)(1, threshold)) || 1
    };
    let isFirstUpdate = true;
    function handleObserve(entries) {
      const ratio = entries[0].intersectionRatio;
      if (ratio !== threshold) {
        if (!isFirstUpdate) {
          return refresh();
        }
        if (!ratio) {
          // If the reference is clipped, the ratio is 0. Throttle the refresh
          // to prevent an infinite loop of updates.
          timeoutId = setTimeout(() => {
            refresh(false, 1e-7);
          }, 1000);
        } else {
          refresh(false, ratio);
        }
      }
      isFirstUpdate = false;
    }

    // Older browsers don't support a `document` as the root and will throw an
    // error.
    try {
      io = new IntersectionObserver(handleObserve, {
        ...options,
        // Handle <iframe>s
        root: root.ownerDocument
      });
    } catch (e) {
      io = new IntersectionObserver(handleObserve, options);
    }
    io.observe(element);
  }
  refresh(true);
  return cleanup;
}

/**
 * Automatically updates the position of the floating element when necessary.
 * Should only be called when the floating element is mounted on the DOM or
 * visible on the screen.
 * @returns cleanup function that should be invoked when the floating element is
 * removed from the DOM or hidden from the screen.
 * @see https://floating-ui.com/docs/autoUpdate
 */
function autoUpdate(reference, floating, update, options) {
  if (options === void 0) {
    options = {};
  }
  const {
    ancestorScroll = true,
    ancestorResize = true,
    elementResize = typeof ResizeObserver === 'function',
    layoutShift = typeof IntersectionObserver === 'function',
    animationFrame = false
  } = options;
  const referenceEl = unwrapElement(reference);
  const ancestors = ancestorScroll || ancestorResize ? [...(referenceEl ? (0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getOverflowAncestors)(referenceEl) : []), ...(0,_floating_ui_utils_dom__WEBPACK_IMPORTED_MODULE_0__.getOverflowAncestors)(floating)] : [];
  ancestors.forEach(ancestor => {
    ancestorScroll && ancestor.addEventListener('scroll', update, {
      passive: true
    });
    ancestorResize && ancestor.addEventListener('resize', update);
  });
  const cleanupIo = referenceEl && layoutShift ? observeMove(referenceEl, update) : null;
  let reobserveFrame = -1;
  let resizeObserver = null;
  if (elementResize) {
    resizeObserver = new ResizeObserver(_ref => {
      let [firstEntry] = _ref;
      if (firstEntry && firstEntry.target === referenceEl && resizeObserver) {
        // Prevent update loops when using the `size` middleware.
        // https://github.com/floating-ui/floating-ui/issues/1740
        resizeObserver.unobserve(floating);
        cancelAnimationFrame(reobserveFrame);
        reobserveFrame = requestAnimationFrame(() => {
          var _resizeObserver;
          (_resizeObserver = resizeObserver) == null || _resizeObserver.observe(floating);
        });
      }
      update();
    });
    if (referenceEl && !animationFrame) {
      resizeObserver.observe(referenceEl);
    }
    resizeObserver.observe(floating);
  }
  let frameId;
  let prevRefRect = animationFrame ? getBoundingClientRect(reference) : null;
  if (animationFrame) {
    frameLoop();
  }
  function frameLoop() {
    const nextRefRect = getBoundingClientRect(reference);
    if (prevRefRect && (nextRefRect.x !== prevRefRect.x || nextRefRect.y !== prevRefRect.y || nextRefRect.width !== prevRefRect.width || nextRefRect.height !== prevRefRect.height)) {
      update();
    }
    prevRefRect = nextRefRect;
    frameId = requestAnimationFrame(frameLoop);
  }
  update();
  return () => {
    var _resizeObserver2;
    ancestors.forEach(ancestor => {
      ancestorScroll && ancestor.removeEventListener('scroll', update);
      ancestorResize && ancestor.removeEventListener('resize', update);
    });
    cleanupIo == null || cleanupIo();
    (_resizeObserver2 = resizeObserver) == null || _resizeObserver2.disconnect();
    resizeObserver = null;
    if (animationFrame) {
      cancelAnimationFrame(frameId);
    }
  };
}

/**
 * Resolves with an object of overflow side offsets that determine how much the
 * element is overflowing a given clipping boundary on each side.
 * - positive = overflowing the boundary by that number of pixels
 * - negative = how many pixels left before it will overflow
 * - 0 = lies flush with the boundary
 * @see https://floating-ui.com/docs/detectOverflow
 */
const detectOverflow = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.detectOverflow;

/**
 * Modifies the placement by translating the floating element along the
 * specified axes.
 * A number (shorthand for `mainAxis` or distance), or an axes configuration
 * object may be passed.
 * @see https://floating-ui.com/docs/offset
 */
const offset = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.offset;

/**
 * Optimizes the visibility of the floating element by choosing the placement
 * that has the most space available automatically, without needing to specify a
 * preferred placement. Alternative to `flip`.
 * @see https://floating-ui.com/docs/autoPlacement
 */
const autoPlacement = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.autoPlacement;

/**
 * Optimizes the visibility of the floating element by shifting it in order to
 * keep it in view when it will overflow the clipping boundary.
 * @see https://floating-ui.com/docs/shift
 */
const shift = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.shift;

/**
 * Optimizes the visibility of the floating element by flipping the `placement`
 * in order to keep it in view when the preferred placement(s) will overflow the
 * clipping boundary. Alternative to `autoPlacement`.
 * @see https://floating-ui.com/docs/flip
 */
const flip = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.flip;

/**
 * Provides data that allows you to change the size of the floating element —
 * for instance, prevent it from overflowing the clipping boundary or match the
 * width of the reference element.
 * @see https://floating-ui.com/docs/size
 */
const size = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.size;

/**
 * Provides data to hide the floating element in applicable situations, such as
 * when it is not in the same clipping context as the reference element.
 * @see https://floating-ui.com/docs/hide
 */
const hide = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.hide;

/**
 * Provides data to position an inner element of the floating element so that it
 * appears centered to the reference element.
 * @see https://floating-ui.com/docs/arrow
 */
const arrow = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.arrow;

/**
 * Provides improved positioning for inline reference elements that can span
 * over multiple lines, such as hyperlinks or range selections.
 * @see https://floating-ui.com/docs/inline
 */
const inline = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.inline;

/**
 * Built-in `limiter` that will stop `shift()` at a certain point.
 */
const limitShift = _floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.limitShift;

/**
 * Computes the `x` and `y` coordinates that will place the floating element
 * next to a given reference element.
 */
const computePosition = (reference, floating, options) => {
  // This caches the expensive `getClippingElementAncestors` function so that
  // multiple lifecycle resets re-use the same result. It only lives for a
  // single call. If other functions become expensive, we can add them as well.
  const cache = new Map();
  const mergedOptions = {
    platform,
    ...options
  };
  const platformWithCache = {
    ...mergedOptions.platform,
    _c: cache
  };
  return (0,_floating_ui_core__WEBPACK_IMPORTED_MODULE_2__.computePosition)(reference, floating, {
    ...mergedOptions,
    platform: platformWithCache
  });
};




/***/ }),

/***/ "./node_modules/alpinejs/dist/module.esm.js":
/*!**************************************************!*\
  !*** ./node_modules/alpinejs/dist/module.esm.js ***!
  \**************************************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "default": () => (/* binding */ module_default)
/* harmony export */ });
// packages/alpinejs/src/scheduler.js
var flushPending = false;
var flushing = false;
var queue = [];
function scheduler(callback) {
  queueJob(callback);
}
function queueJob(job) {
  if (!queue.includes(job))
    queue.push(job);
  queueFlush();
}
function dequeueJob(job) {
  let index = queue.indexOf(job);
  if (index !== -1)
    queue.splice(index, 1);
}
function queueFlush() {
  if (!flushing && !flushPending) {
    flushPending = true;
    queueMicrotask(flushJobs);
  }
}
function flushJobs() {
  flushPending = false;
  flushing = true;
  for (let i = 0; i < queue.length; i++) {
    queue[i]();
  }
  queue.length = 0;
  flushing = false;
}

// packages/alpinejs/src/reactivity.js
var reactive;
var effect;
var release;
var raw;
var shouldSchedule = true;
function disableEffectScheduling(callback) {
  shouldSchedule = false;
  callback();
  shouldSchedule = true;
}
function setReactivityEngine(engine) {
  reactive = engine.reactive;
  release = engine.release;
  effect = (callback) => engine.effect(callback, {scheduler: (task) => {
    if (shouldSchedule) {
      scheduler(task);
    } else {
      task();
    }
  }});
  raw = engine.raw;
}
function overrideEffect(override) {
  effect = override;
}
function elementBoundEffect(el) {
  let cleanup2 = () => {
  };
  let wrappedEffect = (callback) => {
    let effectReference = effect(callback);
    if (!el._x_effects) {
      el._x_effects = new Set();
      el._x_runEffects = () => {
        el._x_effects.forEach((i) => i());
      };
    }
    el._x_effects.add(effectReference);
    cleanup2 = () => {
      if (effectReference === void 0)
        return;
      el._x_effects.delete(effectReference);
      release(effectReference);
    };
    return effectReference;
  };
  return [wrappedEffect, () => {
    cleanup2();
  }];
}

// packages/alpinejs/src/mutation.js
var onAttributeAddeds = [];
var onElRemoveds = [];
var onElAddeds = [];
function onElAdded(callback) {
  onElAddeds.push(callback);
}
function onElRemoved(el, callback) {
  if (typeof callback === "function") {
    if (!el._x_cleanups)
      el._x_cleanups = [];
    el._x_cleanups.push(callback);
  } else {
    callback = el;
    onElRemoveds.push(callback);
  }
}
function onAttributesAdded(callback) {
  onAttributeAddeds.push(callback);
}
function onAttributeRemoved(el, name, callback) {
  if (!el._x_attributeCleanups)
    el._x_attributeCleanups = {};
  if (!el._x_attributeCleanups[name])
    el._x_attributeCleanups[name] = [];
  el._x_attributeCleanups[name].push(callback);
}
function cleanupAttributes(el, names) {
  if (!el._x_attributeCleanups)
    return;
  Object.entries(el._x_attributeCleanups).forEach(([name, value]) => {
    if (names === void 0 || names.includes(name)) {
      value.forEach((i) => i());
      delete el._x_attributeCleanups[name];
    }
  });
}
var observer = new MutationObserver(onMutate);
var currentlyObserving = false;
function startObservingMutations() {
  observer.observe(document, {subtree: true, childList: true, attributes: true, attributeOldValue: true});
  currentlyObserving = true;
}
function stopObservingMutations() {
  flushObserver();
  observer.disconnect();
  currentlyObserving = false;
}
var recordQueue = [];
var willProcessRecordQueue = false;
function flushObserver() {
  recordQueue = recordQueue.concat(observer.takeRecords());
  if (recordQueue.length && !willProcessRecordQueue) {
    willProcessRecordQueue = true;
    queueMicrotask(() => {
      processRecordQueue();
      willProcessRecordQueue = false;
    });
  }
}
function processRecordQueue() {
  onMutate(recordQueue);
  recordQueue.length = 0;
}
function mutateDom(callback) {
  if (!currentlyObserving)
    return callback();
  stopObservingMutations();
  let result = callback();
  startObservingMutations();
  return result;
}
var isCollecting = false;
var deferredMutations = [];
function deferMutations() {
  isCollecting = true;
}
function flushAndStopDeferringMutations() {
  isCollecting = false;
  onMutate(deferredMutations);
  deferredMutations = [];
}
function onMutate(mutations) {
  if (isCollecting) {
    deferredMutations = deferredMutations.concat(mutations);
    return;
  }
  let addedNodes = [];
  let removedNodes = [];
  let addedAttributes = new Map();
  let removedAttributes = new Map();
  for (let i = 0; i < mutations.length; i++) {
    if (mutations[i].target._x_ignoreMutationObserver)
      continue;
    if (mutations[i].type === "childList") {
      mutations[i].addedNodes.forEach((node) => node.nodeType === 1 && addedNodes.push(node));
      mutations[i].removedNodes.forEach((node) => node.nodeType === 1 && removedNodes.push(node));
    }
    if (mutations[i].type === "attributes") {
      let el = mutations[i].target;
      let name = mutations[i].attributeName;
      let oldValue = mutations[i].oldValue;
      let add2 = () => {
        if (!addedAttributes.has(el))
          addedAttributes.set(el, []);
        addedAttributes.get(el).push({name, value: el.getAttribute(name)});
      };
      let remove = () => {
        if (!removedAttributes.has(el))
          removedAttributes.set(el, []);
        removedAttributes.get(el).push(name);
      };
      if (el.hasAttribute(name) && oldValue === null) {
        add2();
      } else if (el.hasAttribute(name)) {
        remove();
        add2();
      } else {
        remove();
      }
    }
  }
  removedAttributes.forEach((attrs, el) => {
    cleanupAttributes(el, attrs);
  });
  addedAttributes.forEach((attrs, el) => {
    onAttributeAddeds.forEach((i) => i(el, attrs));
  });
  for (let node of removedNodes) {
    if (addedNodes.includes(node))
      continue;
    onElRemoveds.forEach((i) => i(node));
    if (node._x_cleanups) {
      while (node._x_cleanups.length)
        node._x_cleanups.pop()();
    }
  }
  addedNodes.forEach((node) => {
    node._x_ignoreSelf = true;
    node._x_ignore = true;
  });
  for (let node of addedNodes) {
    if (removedNodes.includes(node))
      continue;
    if (!node.isConnected)
      continue;
    delete node._x_ignoreSelf;
    delete node._x_ignore;
    onElAddeds.forEach((i) => i(node));
    node._x_ignore = true;
    node._x_ignoreSelf = true;
  }
  addedNodes.forEach((node) => {
    delete node._x_ignoreSelf;
    delete node._x_ignore;
  });
  addedNodes = null;
  removedNodes = null;
  addedAttributes = null;
  removedAttributes = null;
}

// packages/alpinejs/src/scope.js
function scope(node) {
  return mergeProxies(closestDataStack(node));
}
function addScopeToNode(node, data2, referenceNode) {
  node._x_dataStack = [data2, ...closestDataStack(referenceNode || node)];
  return () => {
    node._x_dataStack = node._x_dataStack.filter((i) => i !== data2);
  };
}
function refreshScope(element, scope2) {
  let existingScope = element._x_dataStack[0];
  Object.entries(scope2).forEach(([key, value]) => {
    existingScope[key] = value;
  });
}
function closestDataStack(node) {
  if (node._x_dataStack)
    return node._x_dataStack;
  if (typeof ShadowRoot === "function" && node instanceof ShadowRoot) {
    return closestDataStack(node.host);
  }
  if (!node.parentNode) {
    return [];
  }
  return closestDataStack(node.parentNode);
}
function mergeProxies(objects) {
  let thisProxy = new Proxy({}, {
    ownKeys: () => {
      return Array.from(new Set(objects.flatMap((i) => Object.keys(i))));
    },
    has: (target, name) => {
      return objects.some((obj) => obj.hasOwnProperty(name));
    },
    get: (target, name) => {
      return (objects.find((obj) => {
        if (obj.hasOwnProperty(name)) {
          let descriptor = Object.getOwnPropertyDescriptor(obj, name);
          if (descriptor.get && descriptor.get._x_alreadyBound || descriptor.set && descriptor.set._x_alreadyBound) {
            return true;
          }
          if ((descriptor.get || descriptor.set) && descriptor.enumerable) {
            let getter = descriptor.get;
            let setter = descriptor.set;
            let property = descriptor;
            getter = getter && getter.bind(thisProxy);
            setter = setter && setter.bind(thisProxy);
            if (getter)
              getter._x_alreadyBound = true;
            if (setter)
              setter._x_alreadyBound = true;
            Object.defineProperty(obj, name, {
              ...property,
              get: getter,
              set: setter
            });
          }
          return true;
        }
        return false;
      }) || {})[name];
    },
    set: (target, name, value) => {
      let closestObjectWithKey = objects.find((obj) => obj.hasOwnProperty(name));
      if (closestObjectWithKey) {
        closestObjectWithKey[name] = value;
      } else {
        objects[objects.length - 1][name] = value;
      }
      return true;
    }
  });
  return thisProxy;
}

// packages/alpinejs/src/interceptor.js
function initInterceptors(data2) {
  let isObject2 = (val) => typeof val === "object" && !Array.isArray(val) && val !== null;
  let recurse = (obj, basePath = "") => {
    Object.entries(Object.getOwnPropertyDescriptors(obj)).forEach(([key, {value, enumerable}]) => {
      if (enumerable === false || value === void 0)
        return;
      let path = basePath === "" ? key : `${basePath}.${key}`;
      if (typeof value === "object" && value !== null && value._x_interceptor) {
        obj[key] = value.initialize(data2, path, key);
      } else {
        if (isObject2(value) && value !== obj && !(value instanceof Element)) {
          recurse(value, path);
        }
      }
    });
  };
  return recurse(data2);
}
function interceptor(callback, mutateObj = () => {
}) {
  let obj = {
    initialValue: void 0,
    _x_interceptor: true,
    initialize(data2, path, key) {
      return callback(this.initialValue, () => get(data2, path), (value) => set(data2, path, value), path, key);
    }
  };
  mutateObj(obj);
  return (initialValue) => {
    if (typeof initialValue === "object" && initialValue !== null && initialValue._x_interceptor) {
      let initialize = obj.initialize.bind(obj);
      obj.initialize = (data2, path, key) => {
        let innerValue = initialValue.initialize(data2, path, key);
        obj.initialValue = innerValue;
        return initialize(data2, path, key);
      };
    } else {
      obj.initialValue = initialValue;
    }
    return obj;
  };
}
function get(obj, path) {
  return path.split(".").reduce((carry, segment) => carry[segment], obj);
}
function set(obj, path, value) {
  if (typeof path === "string")
    path = path.split(".");
  if (path.length === 1)
    obj[path[0]] = value;
  else if (path.length === 0)
    throw error;
  else {
    if (obj[path[0]])
      return set(obj[path[0]], path.slice(1), value);
    else {
      obj[path[0]] = {};
      return set(obj[path[0]], path.slice(1), value);
    }
  }
}

// packages/alpinejs/src/magics.js
var magics = {};
function magic(name, callback) {
  magics[name] = callback;
}
function injectMagics(obj, el) {
  Object.entries(magics).forEach(([name, callback]) => {
    Object.defineProperty(obj, `$${name}`, {
      get() {
        let [utilities, cleanup2] = getElementBoundUtilities(el);
        utilities = {interceptor, ...utilities};
        onElRemoved(el, cleanup2);
        return callback(el, utilities);
      },
      enumerable: false
    });
  });
  return obj;
}

// packages/alpinejs/src/utils/error.js
function tryCatch(el, expression, callback, ...args) {
  try {
    return callback(...args);
  } catch (e) {
    handleError(e, el, expression);
  }
}
function handleError(error2, el, expression = void 0) {
  Object.assign(error2, {el, expression});
  console.warn(`Alpine Expression Error: ${error2.message}

${expression ? 'Expression: "' + expression + '"\n\n' : ""}`, el);
  setTimeout(() => {
    throw error2;
  }, 0);
}

// packages/alpinejs/src/evaluator.js
var shouldAutoEvaluateFunctions = true;
function dontAutoEvaluateFunctions(callback) {
  let cache = shouldAutoEvaluateFunctions;
  shouldAutoEvaluateFunctions = false;
  callback();
  shouldAutoEvaluateFunctions = cache;
}
function evaluate(el, expression, extras = {}) {
  let result;
  evaluateLater(el, expression)((value) => result = value, extras);
  return result;
}
function evaluateLater(...args) {
  return theEvaluatorFunction(...args);
}
var theEvaluatorFunction = normalEvaluator;
function setEvaluator(newEvaluator) {
  theEvaluatorFunction = newEvaluator;
}
function normalEvaluator(el, expression) {
  let overriddenMagics = {};
  injectMagics(overriddenMagics, el);
  let dataStack = [overriddenMagics, ...closestDataStack(el)];
  if (typeof expression === "function") {
    return generateEvaluatorFromFunction(dataStack, expression);
  }
  let evaluator = generateEvaluatorFromString(dataStack, expression, el);
  return tryCatch.bind(null, el, expression, evaluator);
}
function generateEvaluatorFromFunction(dataStack, func) {
  return (receiver = () => {
  }, {scope: scope2 = {}, params = []} = {}) => {
    let result = func.apply(mergeProxies([scope2, ...dataStack]), params);
    runIfTypeOfFunction(receiver, result);
  };
}
var evaluatorMemo = {};
function generateFunctionFromString(expression, el) {
  if (evaluatorMemo[expression]) {
    return evaluatorMemo[expression];
  }
  let AsyncFunction = Object.getPrototypeOf(async function() {
  }).constructor;
  let rightSideSafeExpression = /^[\n\s]*if.*\(.*\)/.test(expression) || /^(let|const)\s/.test(expression) ? `(() => { ${expression} })()` : expression;
  const safeAsyncFunction = () => {
    try {
      return new AsyncFunction(["__self", "scope"], `with (scope) { __self.result = ${rightSideSafeExpression} }; __self.finished = true; return __self.result;`);
    } catch (error2) {
      handleError(error2, el, expression);
      return Promise.resolve();
    }
  };
  let func = safeAsyncFunction();
  evaluatorMemo[expression] = func;
  return func;
}
function generateEvaluatorFromString(dataStack, expression, el) {
  let func = generateFunctionFromString(expression, el);
  return (receiver = () => {
  }, {scope: scope2 = {}, params = []} = {}) => {
    func.result = void 0;
    func.finished = false;
    let completeScope = mergeProxies([scope2, ...dataStack]);
    if (typeof func === "function") {
      let promise = func(func, completeScope).catch((error2) => handleError(error2, el, expression));
      if (func.finished) {
        runIfTypeOfFunction(receiver, func.result, completeScope, params, el);
        func.result = void 0;
      } else {
        promise.then((result) => {
          runIfTypeOfFunction(receiver, result, completeScope, params, el);
        }).catch((error2) => handleError(error2, el, expression)).finally(() => func.result = void 0);
      }
    }
  };
}
function runIfTypeOfFunction(receiver, value, scope2, params, el) {
  if (shouldAutoEvaluateFunctions && typeof value === "function") {
    let result = value.apply(scope2, params);
    if (result instanceof Promise) {
      result.then((i) => runIfTypeOfFunction(receiver, i, scope2, params)).catch((error2) => handleError(error2, el, value));
    } else {
      receiver(result);
    }
  } else {
    receiver(value);
  }
}

// packages/alpinejs/src/directives.js
var prefixAsString = "x-";
function prefix(subject = "") {
  return prefixAsString + subject;
}
function setPrefix(newPrefix) {
  prefixAsString = newPrefix;
}
var directiveHandlers = {};
function directive(name, callback) {
  directiveHandlers[name] = callback;
}
function directives(el, attributes, originalAttributeOverride) {
  attributes = Array.from(attributes);
  if (el._x_virtualDirectives) {
    let vAttributes = Object.entries(el._x_virtualDirectives).map(([name, value]) => ({name, value}));
    let staticAttributes = attributesOnly(vAttributes);
    vAttributes = vAttributes.map((attribute) => {
      if (staticAttributes.find((attr) => attr.name === attribute.name)) {
        return {
          name: `x-bind:${attribute.name}`,
          value: `"${attribute.value}"`
        };
      }
      return attribute;
    });
    attributes = attributes.concat(vAttributes);
  }
  let transformedAttributeMap = {};
  let directives2 = attributes.map(toTransformedAttributes((newName, oldName) => transformedAttributeMap[newName] = oldName)).filter(outNonAlpineAttributes).map(toParsedDirectives(transformedAttributeMap, originalAttributeOverride)).sort(byPriority);
  return directives2.map((directive2) => {
    return getDirectiveHandler(el, directive2);
  });
}
function attributesOnly(attributes) {
  return Array.from(attributes).map(toTransformedAttributes()).filter((attr) => !outNonAlpineAttributes(attr));
}
var isDeferringHandlers = false;
var directiveHandlerStacks = new Map();
var currentHandlerStackKey = Symbol();
function deferHandlingDirectives(callback) {
  isDeferringHandlers = true;
  let key = Symbol();
  currentHandlerStackKey = key;
  directiveHandlerStacks.set(key, []);
  let flushHandlers = () => {
    while (directiveHandlerStacks.get(key).length)
      directiveHandlerStacks.get(key).shift()();
    directiveHandlerStacks.delete(key);
  };
  let stopDeferring = () => {
    isDeferringHandlers = false;
    flushHandlers();
  };
  callback(flushHandlers);
  stopDeferring();
}
function getElementBoundUtilities(el) {
  let cleanups = [];
  let cleanup2 = (callback) => cleanups.push(callback);
  let [effect3, cleanupEffect] = elementBoundEffect(el);
  cleanups.push(cleanupEffect);
  let utilities = {
    Alpine: alpine_default,
    effect: effect3,
    cleanup: cleanup2,
    evaluateLater: evaluateLater.bind(evaluateLater, el),
    evaluate: evaluate.bind(evaluate, el)
  };
  let doCleanup = () => cleanups.forEach((i) => i());
  return [utilities, doCleanup];
}
function getDirectiveHandler(el, directive2) {
  let noop = () => {
  };
  let handler3 = directiveHandlers[directive2.type] || noop;
  let [utilities, cleanup2] = getElementBoundUtilities(el);
  onAttributeRemoved(el, directive2.original, cleanup2);
  let fullHandler = () => {
    if (el._x_ignore || el._x_ignoreSelf)
      return;
    handler3.inline && handler3.inline(el, directive2, utilities);
    handler3 = handler3.bind(handler3, el, directive2, utilities);
    isDeferringHandlers ? directiveHandlerStacks.get(currentHandlerStackKey).push(handler3) : handler3();
  };
  fullHandler.runCleanups = cleanup2;
  return fullHandler;
}
var startingWith = (subject, replacement) => ({name, value}) => {
  if (name.startsWith(subject))
    name = name.replace(subject, replacement);
  return {name, value};
};
var into = (i) => i;
function toTransformedAttributes(callback = () => {
}) {
  return ({name, value}) => {
    let {name: newName, value: newValue} = attributeTransformers.reduce((carry, transform) => {
      return transform(carry);
    }, {name, value});
    if (newName !== name)
      callback(newName, name);
    return {name: newName, value: newValue};
  };
}
var attributeTransformers = [];
function mapAttributes(callback) {
  attributeTransformers.push(callback);
}
function outNonAlpineAttributes({name}) {
  return alpineAttributeRegex().test(name);
}
var alpineAttributeRegex = () => new RegExp(`^${prefixAsString}([^:^.]+)\\b`);
function toParsedDirectives(transformedAttributeMap, originalAttributeOverride) {
  return ({name, value}) => {
    let typeMatch = name.match(alpineAttributeRegex());
    let valueMatch = name.match(/:([a-zA-Z0-9\-:]+)/);
    let modifiers = name.match(/\.[^.\]]+(?=[^\]]*$)/g) || [];
    let original = originalAttributeOverride || transformedAttributeMap[name] || name;
    return {
      type: typeMatch ? typeMatch[1] : null,
      value: valueMatch ? valueMatch[1] : null,
      modifiers: modifiers.map((i) => i.replace(".", "")),
      expression: value,
      original
    };
  };
}
var DEFAULT = "DEFAULT";
var directiveOrder = [
  "ignore",
  "ref",
  "data",
  "id",
  "radio",
  "tabs",
  "switch",
  "disclosure",
  "menu",
  "listbox",
  "list",
  "item",
  "combobox",
  "bind",
  "init",
  "for",
  "mask",
  "model",
  "modelable",
  "transition",
  "show",
  "if",
  DEFAULT,
  "teleport"
];
function byPriority(a, b) {
  let typeA = directiveOrder.indexOf(a.type) === -1 ? DEFAULT : a.type;
  let typeB = directiveOrder.indexOf(b.type) === -1 ? DEFAULT : b.type;
  return directiveOrder.indexOf(typeA) - directiveOrder.indexOf(typeB);
}

// packages/alpinejs/src/utils/dispatch.js
function dispatch(el, name, detail = {}) {
  el.dispatchEvent(new CustomEvent(name, {
    detail,
    bubbles: true,
    composed: true,
    cancelable: true
  }));
}

// packages/alpinejs/src/nextTick.js
var tickStack = [];
var isHolding = false;
function nextTick(callback = () => {
}) {
  queueMicrotask(() => {
    isHolding || setTimeout(() => {
      releaseNextTicks();
    });
  });
  return new Promise((res) => {
    tickStack.push(() => {
      callback();
      res();
    });
  });
}
function releaseNextTicks() {
  isHolding = false;
  while (tickStack.length)
    tickStack.shift()();
}
function holdNextTicks() {
  isHolding = true;
}

// packages/alpinejs/src/utils/walk.js
function walk(el, callback) {
  if (typeof ShadowRoot === "function" && el instanceof ShadowRoot) {
    Array.from(el.children).forEach((el2) => walk(el2, callback));
    return;
  }
  let skip = false;
  callback(el, () => skip = true);
  if (skip)
    return;
  let node = el.firstElementChild;
  while (node) {
    walk(node, callback, false);
    node = node.nextElementSibling;
  }
}

// packages/alpinejs/src/utils/warn.js
function warn(message, ...args) {
  console.warn(`Alpine Warning: ${message}`, ...args);
}

// packages/alpinejs/src/lifecycle.js
function start() {
  if (!document.body)
    warn("Unable to initialize. Trying to load Alpine before `<body>` is available. Did you forget to add `defer` in Alpine's `<script>` tag?");
  dispatch(document, "alpine:init");
  dispatch(document, "alpine:initializing");
  startObservingMutations();
  onElAdded((el) => initTree(el, walk));
  onElRemoved((el) => destroyTree(el));
  onAttributesAdded((el, attrs) => {
    directives(el, attrs).forEach((handle) => handle());
  });
  let outNestedComponents = (el) => !closestRoot(el.parentElement, true);
  Array.from(document.querySelectorAll(allSelectors())).filter(outNestedComponents).forEach((el) => {
    initTree(el);
  });
  dispatch(document, "alpine:initialized");
}
var rootSelectorCallbacks = [];
var initSelectorCallbacks = [];
function rootSelectors() {
  return rootSelectorCallbacks.map((fn) => fn());
}
function allSelectors() {
  return rootSelectorCallbacks.concat(initSelectorCallbacks).map((fn) => fn());
}
function addRootSelector(selectorCallback) {
  rootSelectorCallbacks.push(selectorCallback);
}
function addInitSelector(selectorCallback) {
  initSelectorCallbacks.push(selectorCallback);
}
function closestRoot(el, includeInitSelectors = false) {
  return findClosest(el, (element) => {
    const selectors = includeInitSelectors ? allSelectors() : rootSelectors();
    if (selectors.some((selector) => element.matches(selector)))
      return true;
  });
}
function findClosest(el, callback) {
  if (!el)
    return;
  if (callback(el))
    return el;
  if (el._x_teleportBack)
    el = el._x_teleportBack;
  if (!el.parentElement)
    return;
  return findClosest(el.parentElement, callback);
}
function isRoot(el) {
  return rootSelectors().some((selector) => el.matches(selector));
}
function initTree(el, walker = walk) {
  deferHandlingDirectives(() => {
    walker(el, (el2, skip) => {
      directives(el2, el2.attributes).forEach((handle) => handle());
      el2._x_ignore && skip();
    });
  });
}
function destroyTree(root) {
  walk(root, (el) => cleanupAttributes(el));
}

// packages/alpinejs/src/utils/classes.js
function setClasses(el, value) {
  if (Array.isArray(value)) {
    return setClassesFromString(el, value.join(" "));
  } else if (typeof value === "object" && value !== null) {
    return setClassesFromObject(el, value);
  } else if (typeof value === "function") {
    return setClasses(el, value());
  }
  return setClassesFromString(el, value);
}
function setClassesFromString(el, classString) {
  let split = (classString2) => classString2.split(" ").filter(Boolean);
  let missingClasses = (classString2) => classString2.split(" ").filter((i) => !el.classList.contains(i)).filter(Boolean);
  let addClassesAndReturnUndo = (classes) => {
    el.classList.add(...classes);
    return () => {
      el.classList.remove(...classes);
    };
  };
  classString = classString === true ? classString = "" : classString || "";
  return addClassesAndReturnUndo(missingClasses(classString));
}
function setClassesFromObject(el, classObject) {
  let split = (classString) => classString.split(" ").filter(Boolean);
  let forAdd = Object.entries(classObject).flatMap(([classString, bool]) => bool ? split(classString) : false).filter(Boolean);
  let forRemove = Object.entries(classObject).flatMap(([classString, bool]) => !bool ? split(classString) : false).filter(Boolean);
  let added = [];
  let removed = [];
  forRemove.forEach((i) => {
    if (el.classList.contains(i)) {
      el.classList.remove(i);
      removed.push(i);
    }
  });
  forAdd.forEach((i) => {
    if (!el.classList.contains(i)) {
      el.classList.add(i);
      added.push(i);
    }
  });
  return () => {
    removed.forEach((i) => el.classList.add(i));
    added.forEach((i) => el.classList.remove(i));
  };
}

// packages/alpinejs/src/utils/styles.js
function setStyles(el, value) {
  if (typeof value === "object" && value !== null) {
    return setStylesFromObject(el, value);
  }
  return setStylesFromString(el, value);
}
function setStylesFromObject(el, value) {
  let previousStyles = {};
  Object.entries(value).forEach(([key, value2]) => {
    previousStyles[key] = el.style[key];
    if (!key.startsWith("--")) {
      key = kebabCase(key);
    }
    el.style.setProperty(key, value2);
  });
  setTimeout(() => {
    if (el.style.length === 0) {
      el.removeAttribute("style");
    }
  });
  return () => {
    setStyles(el, previousStyles);
  };
}
function setStylesFromString(el, value) {
  let cache = el.getAttribute("style", value);
  el.setAttribute("style", value);
  return () => {
    el.setAttribute("style", cache || "");
  };
}
function kebabCase(subject) {
  return subject.replace(/([a-z])([A-Z])/g, "$1-$2").toLowerCase();
}

// packages/alpinejs/src/utils/once.js
function once(callback, fallback = () => {
}) {
  let called = false;
  return function() {
    if (!called) {
      called = true;
      callback.apply(this, arguments);
    } else {
      fallback.apply(this, arguments);
    }
  };
}

// packages/alpinejs/src/directives/x-transition.js
directive("transition", (el, {value, modifiers, expression}, {evaluate: evaluate2}) => {
  if (typeof expression === "function")
    expression = evaluate2(expression);
  if (!expression) {
    registerTransitionsFromHelper(el, modifiers, value);
  } else {
    registerTransitionsFromClassString(el, expression, value);
  }
});
function registerTransitionsFromClassString(el, classString, stage) {
  registerTransitionObject(el, setClasses, "");
  let directiveStorageMap = {
    enter: (classes) => {
      el._x_transition.enter.during = classes;
    },
    "enter-start": (classes) => {
      el._x_transition.enter.start = classes;
    },
    "enter-end": (classes) => {
      el._x_transition.enter.end = classes;
    },
    leave: (classes) => {
      el._x_transition.leave.during = classes;
    },
    "leave-start": (classes) => {
      el._x_transition.leave.start = classes;
    },
    "leave-end": (classes) => {
      el._x_transition.leave.end = classes;
    }
  };
  directiveStorageMap[stage](classString);
}
function registerTransitionsFromHelper(el, modifiers, stage) {
  registerTransitionObject(el, setStyles);
  let doesntSpecify = !modifiers.includes("in") && !modifiers.includes("out") && !stage;
  let transitioningIn = doesntSpecify || modifiers.includes("in") || ["enter"].includes(stage);
  let transitioningOut = doesntSpecify || modifiers.includes("out") || ["leave"].includes(stage);
  if (modifiers.includes("in") && !doesntSpecify) {
    modifiers = modifiers.filter((i, index) => index < modifiers.indexOf("out"));
  }
  if (modifiers.includes("out") && !doesntSpecify) {
    modifiers = modifiers.filter((i, index) => index > modifiers.indexOf("out"));
  }
  let wantsAll = !modifiers.includes("opacity") && !modifiers.includes("scale");
  let wantsOpacity = wantsAll || modifiers.includes("opacity");
  let wantsScale = wantsAll || modifiers.includes("scale");
  let opacityValue = wantsOpacity ? 0 : 1;
  let scaleValue = wantsScale ? modifierValue(modifiers, "scale", 95) / 100 : 1;
  let delay = modifierValue(modifiers, "delay", 0);
  let origin = modifierValue(modifiers, "origin", "center");
  let property = "opacity, transform";
  let durationIn = modifierValue(modifiers, "duration", 150) / 1e3;
  let durationOut = modifierValue(modifiers, "duration", 75) / 1e3;
  let easing = `cubic-bezier(0.4, 0.0, 0.2, 1)`;
  if (transitioningIn) {
    el._x_transition.enter.during = {
      transformOrigin: origin,
      transitionDelay: delay,
      transitionProperty: property,
      transitionDuration: `${durationIn}s`,
      transitionTimingFunction: easing
    };
    el._x_transition.enter.start = {
      opacity: opacityValue,
      transform: `scale(${scaleValue})`
    };
    el._x_transition.enter.end = {
      opacity: 1,
      transform: `scale(1)`
    };
  }
  if (transitioningOut) {
    el._x_transition.leave.during = {
      transformOrigin: origin,
      transitionDelay: delay,
      transitionProperty: property,
      transitionDuration: `${durationOut}s`,
      transitionTimingFunction: easing
    };
    el._x_transition.leave.start = {
      opacity: 1,
      transform: `scale(1)`
    };
    el._x_transition.leave.end = {
      opacity: opacityValue,
      transform: `scale(${scaleValue})`
    };
  }
}
function registerTransitionObject(el, setFunction, defaultValue = {}) {
  if (!el._x_transition)
    el._x_transition = {
      enter: {during: defaultValue, start: defaultValue, end: defaultValue},
      leave: {during: defaultValue, start: defaultValue, end: defaultValue},
      in(before = () => {
      }, after = () => {
      }) {
        transition(el, setFunction, {
          during: this.enter.during,
          start: this.enter.start,
          end: this.enter.end
        }, before, after);
      },
      out(before = () => {
      }, after = () => {
      }) {
        transition(el, setFunction, {
          during: this.leave.during,
          start: this.leave.start,
          end: this.leave.end
        }, before, after);
      }
    };
}
window.Element.prototype._x_toggleAndCascadeWithTransitions = function(el, value, show, hide) {
  const nextTick2 = document.visibilityState === "visible" ? requestAnimationFrame : setTimeout;
  let clickAwayCompatibleShow = () => nextTick2(show);
  if (value) {
    if (el._x_transition && (el._x_transition.enter || el._x_transition.leave)) {
      el._x_transition.enter && (Object.entries(el._x_transition.enter.during).length || Object.entries(el._x_transition.enter.start).length || Object.entries(el._x_transition.enter.end).length) ? el._x_transition.in(show) : clickAwayCompatibleShow();
    } else {
      el._x_transition ? el._x_transition.in(show) : clickAwayCompatibleShow();
    }
    return;
  }
  el._x_hidePromise = el._x_transition ? new Promise((resolve, reject) => {
    el._x_transition.out(() => {
    }, () => resolve(hide));
    el._x_transitioning.beforeCancel(() => reject({isFromCancelledTransition: true}));
  }) : Promise.resolve(hide);
  queueMicrotask(() => {
    let closest = closestHide(el);
    if (closest) {
      if (!closest._x_hideChildren)
        closest._x_hideChildren = [];
      closest._x_hideChildren.push(el);
    } else {
      nextTick2(() => {
        let hideAfterChildren = (el2) => {
          let carry = Promise.all([
            el2._x_hidePromise,
            ...(el2._x_hideChildren || []).map(hideAfterChildren)
          ]).then(([i]) => i());
          delete el2._x_hidePromise;
          delete el2._x_hideChildren;
          return carry;
        };
        hideAfterChildren(el).catch((e) => {
          if (!e.isFromCancelledTransition)
            throw e;
        });
      });
    }
  });
};
function closestHide(el) {
  let parent = el.parentNode;
  if (!parent)
    return;
  return parent._x_hidePromise ? parent : closestHide(parent);
}
function transition(el, setFunction, {during, start: start2, end} = {}, before = () => {
}, after = () => {
}) {
  if (el._x_transitioning)
    el._x_transitioning.cancel();
  if (Object.keys(during).length === 0 && Object.keys(start2).length === 0 && Object.keys(end).length === 0) {
    before();
    after();
    return;
  }
  let undoStart, undoDuring, undoEnd;
  performTransition(el, {
    start() {
      undoStart = setFunction(el, start2);
    },
    during() {
      undoDuring = setFunction(el, during);
    },
    before,
    end() {
      undoStart();
      undoEnd = setFunction(el, end);
    },
    after,
    cleanup() {
      undoDuring();
      undoEnd();
    }
  });
}
function performTransition(el, stages) {
  let interrupted, reachedBefore, reachedEnd;
  let finish = once(() => {
    mutateDom(() => {
      interrupted = true;
      if (!reachedBefore)
        stages.before();
      if (!reachedEnd) {
        stages.end();
        releaseNextTicks();
      }
      stages.after();
      if (el.isConnected)
        stages.cleanup();
      delete el._x_transitioning;
    });
  });
  el._x_transitioning = {
    beforeCancels: [],
    beforeCancel(callback) {
      this.beforeCancels.push(callback);
    },
    cancel: once(function() {
      while (this.beforeCancels.length) {
        this.beforeCancels.shift()();
      }
      ;
      finish();
    }),
    finish
  };
  mutateDom(() => {
    stages.start();
    stages.during();
  });
  holdNextTicks();
  requestAnimationFrame(() => {
    if (interrupted)
      return;
    let duration = Number(getComputedStyle(el).transitionDuration.replace(/,.*/, "").replace("s", "")) * 1e3;
    let delay = Number(getComputedStyle(el).transitionDelay.replace(/,.*/, "").replace("s", "")) * 1e3;
    if (duration === 0)
      duration = Number(getComputedStyle(el).animationDuration.replace("s", "")) * 1e3;
    mutateDom(() => {
      stages.before();
    });
    reachedBefore = true;
    requestAnimationFrame(() => {
      if (interrupted)
        return;
      mutateDom(() => {
        stages.end();
      });
      releaseNextTicks();
      setTimeout(el._x_transitioning.finish, duration + delay);
      reachedEnd = true;
    });
  });
}
function modifierValue(modifiers, key, fallback) {
  if (modifiers.indexOf(key) === -1)
    return fallback;
  const rawValue = modifiers[modifiers.indexOf(key) + 1];
  if (!rawValue)
    return fallback;
  if (key === "scale") {
    if (isNaN(rawValue))
      return fallback;
  }
  if (key === "duration") {
    let match = rawValue.match(/([0-9]+)ms/);
    if (match)
      return match[1];
  }
  if (key === "origin") {
    if (["top", "right", "left", "center", "bottom"].includes(modifiers[modifiers.indexOf(key) + 2])) {
      return [rawValue, modifiers[modifiers.indexOf(key) + 2]].join(" ");
    }
  }
  return rawValue;
}

// packages/alpinejs/src/clone.js
var isCloning = false;
function skipDuringClone(callback, fallback = () => {
}) {
  return (...args) => isCloning ? fallback(...args) : callback(...args);
}
function clone(oldEl, newEl) {
  if (!newEl._x_dataStack)
    newEl._x_dataStack = oldEl._x_dataStack;
  isCloning = true;
  dontRegisterReactiveSideEffects(() => {
    cloneTree(newEl);
  });
  isCloning = false;
}
function cloneTree(el) {
  let hasRunThroughFirstEl = false;
  let shallowWalker = (el2, callback) => {
    walk(el2, (el3, skip) => {
      if (hasRunThroughFirstEl && isRoot(el3))
        return skip();
      hasRunThroughFirstEl = true;
      callback(el3, skip);
    });
  };
  initTree(el, shallowWalker);
}
function dontRegisterReactiveSideEffects(callback) {
  let cache = effect;
  overrideEffect((callback2, el) => {
    let storedEffect = cache(callback2);
    release(storedEffect);
    return () => {
    };
  });
  callback();
  overrideEffect(cache);
}

// packages/alpinejs/src/utils/bind.js
function bind(el, name, value, modifiers = []) {
  if (!el._x_bindings)
    el._x_bindings = reactive({});
  el._x_bindings[name] = value;
  name = modifiers.includes("camel") ? camelCase(name) : name;
  switch (name) {
    case "value":
      bindInputValue(el, value);
      break;
    case "style":
      bindStyles(el, value);
      break;
    case "class":
      bindClasses(el, value);
      break;
    default:
      bindAttribute(el, name, value);
      break;
  }
}
function bindInputValue(el, value) {
  if (el.type === "radio") {
    if (el.attributes.value === void 0) {
      el.value = value;
    }
    if (window.fromModel) {
      el.checked = checkedAttrLooseCompare(el.value, value);
    }
  } else if (el.type === "checkbox") {
    if (Number.isInteger(value)) {
      el.value = value;
    } else if (!Number.isInteger(value) && !Array.isArray(value) && typeof value !== "boolean" && ![null, void 0].includes(value)) {
      el.value = String(value);
    } else {
      if (Array.isArray(value)) {
        el.checked = value.some((val) => checkedAttrLooseCompare(val, el.value));
      } else {
        el.checked = !!value;
      }
    }
  } else if (el.tagName === "SELECT") {
    updateSelect(el, value);
  } else {
    if (el.value === value)
      return;
    el.value = value;
  }
}
function bindClasses(el, value) {
  if (el._x_undoAddedClasses)
    el._x_undoAddedClasses();
  el._x_undoAddedClasses = setClasses(el, value);
}
function bindStyles(el, value) {
  if (el._x_undoAddedStyles)
    el._x_undoAddedStyles();
  el._x_undoAddedStyles = setStyles(el, value);
}
function bindAttribute(el, name, value) {
  if ([null, void 0, false].includes(value) && attributeShouldntBePreservedIfFalsy(name)) {
    el.removeAttribute(name);
  } else {
    if (isBooleanAttr(name))
      value = name;
    setIfChanged(el, name, value);
  }
}
function setIfChanged(el, attrName, value) {
  if (el.getAttribute(attrName) != value) {
    el.setAttribute(attrName, value);
  }
}
function updateSelect(el, value) {
  const arrayWrappedValue = [].concat(value).map((value2) => {
    return value2 + "";
  });
  Array.from(el.options).forEach((option) => {
    option.selected = arrayWrappedValue.includes(option.value);
  });
}
function camelCase(subject) {
  return subject.toLowerCase().replace(/-(\w)/g, (match, char) => char.toUpperCase());
}
function checkedAttrLooseCompare(valueA, valueB) {
  return valueA == valueB;
}
function isBooleanAttr(attrName) {
  const booleanAttributes = [
    "disabled",
    "checked",
    "required",
    "readonly",
    "hidden",
    "open",
    "selected",
    "autofocus",
    "itemscope",
    "multiple",
    "novalidate",
    "allowfullscreen",
    "allowpaymentrequest",
    "formnovalidate",
    "autoplay",
    "controls",
    "loop",
    "muted",
    "playsinline",
    "default",
    "ismap",
    "reversed",
    "async",
    "defer",
    "nomodule"
  ];
  return booleanAttributes.includes(attrName);
}
function attributeShouldntBePreservedIfFalsy(name) {
  return !["aria-pressed", "aria-checked", "aria-expanded", "aria-selected"].includes(name);
}
function getBinding(el, name, fallback) {
  if (el._x_bindings && el._x_bindings[name] !== void 0)
    return el._x_bindings[name];
  let attr = el.getAttribute(name);
  if (attr === null)
    return typeof fallback === "function" ? fallback() : fallback;
  if (attr === "")
    return true;
  if (isBooleanAttr(name)) {
    return !![name, "true"].includes(attr);
  }
  return attr;
}

// packages/alpinejs/src/utils/debounce.js
function debounce(func, wait) {
  var timeout;
  return function() {
    var context = this, args = arguments;
    var later = function() {
      timeout = null;
      func.apply(context, args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

// packages/alpinejs/src/utils/throttle.js
function throttle(func, limit) {
  let inThrottle;
  return function() {
    let context = this, args = arguments;
    if (!inThrottle) {
      func.apply(context, args);
      inThrottle = true;
      setTimeout(() => inThrottle = false, limit);
    }
  };
}

// packages/alpinejs/src/plugin.js
function plugin(callback) {
  callback(alpine_default);
}

// packages/alpinejs/src/store.js
var stores = {};
var isReactive = false;
function store(name, value) {
  if (!isReactive) {
    stores = reactive(stores);
    isReactive = true;
  }
  if (value === void 0) {
    return stores[name];
  }
  stores[name] = value;
  if (typeof value === "object" && value !== null && value.hasOwnProperty("init") && typeof value.init === "function") {
    stores[name].init();
  }
  initInterceptors(stores[name]);
}
function getStores() {
  return stores;
}

// packages/alpinejs/src/binds.js
var binds = {};
function bind2(name, bindings) {
  let getBindings = typeof bindings !== "function" ? () => bindings : bindings;
  if (name instanceof Element) {
    applyBindingsObject(name, getBindings());
  } else {
    binds[name] = getBindings;
  }
}
function injectBindingProviders(obj) {
  Object.entries(binds).forEach(([name, callback]) => {
    Object.defineProperty(obj, name, {
      get() {
        return (...args) => {
          return callback(...args);
        };
      }
    });
  });
  return obj;
}
function applyBindingsObject(el, obj, original) {
  let cleanupRunners = [];
  while (cleanupRunners.length)
    cleanupRunners.pop()();
  let attributes = Object.entries(obj).map(([name, value]) => ({name, value}));
  let staticAttributes = attributesOnly(attributes);
  attributes = attributes.map((attribute) => {
    if (staticAttributes.find((attr) => attr.name === attribute.name)) {
      return {
        name: `x-bind:${attribute.name}`,
        value: `"${attribute.value}"`
      };
    }
    return attribute;
  });
  directives(el, attributes, original).map((handle) => {
    cleanupRunners.push(handle.runCleanups);
    handle();
  });
}

// packages/alpinejs/src/datas.js
var datas = {};
function data(name, callback) {
  datas[name] = callback;
}
function injectDataProviders(obj, context) {
  Object.entries(datas).forEach(([name, callback]) => {
    Object.defineProperty(obj, name, {
      get() {
        return (...args) => {
          return callback.bind(context)(...args);
        };
      },
      enumerable: false
    });
  });
  return obj;
}

// packages/alpinejs/src/alpine.js
var Alpine = {
  get reactive() {
    return reactive;
  },
  get release() {
    return release;
  },
  get effect() {
    return effect;
  },
  get raw() {
    return raw;
  },
  version: "3.10.5",
  flushAndStopDeferringMutations,
  dontAutoEvaluateFunctions,
  disableEffectScheduling,
  setReactivityEngine,
  closestDataStack,
  skipDuringClone,
  addRootSelector,
  addInitSelector,
  addScopeToNode,
  deferMutations,
  mapAttributes,
  evaluateLater,
  setEvaluator,
  mergeProxies,
  findClosest,
  closestRoot,
  interceptor,
  transition,
  setStyles,
  mutateDom,
  directive,
  throttle,
  debounce,
  evaluate,
  initTree,
  nextTick,
  prefixed: prefix,
  prefix: setPrefix,
  plugin,
  magic,
  store,
  start,
  clone,
  bound: getBinding,
  $data: scope,
  data,
  bind: bind2
};
var alpine_default = Alpine;

// node_modules/@vue/shared/dist/shared.esm-bundler.js
function makeMap(str, expectsLowerCase) {
  const map = Object.create(null);
  const list = str.split(",");
  for (let i = 0; i < list.length; i++) {
    map[list[i]] = true;
  }
  return expectsLowerCase ? (val) => !!map[val.toLowerCase()] : (val) => !!map[val];
}
var PatchFlagNames = {
  [1]: `TEXT`,
  [2]: `CLASS`,
  [4]: `STYLE`,
  [8]: `PROPS`,
  [16]: `FULL_PROPS`,
  [32]: `HYDRATE_EVENTS`,
  [64]: `STABLE_FRAGMENT`,
  [128]: `KEYED_FRAGMENT`,
  [256]: `UNKEYED_FRAGMENT`,
  [512]: `NEED_PATCH`,
  [1024]: `DYNAMIC_SLOTS`,
  [2048]: `DEV_ROOT_FRAGMENT`,
  [-1]: `HOISTED`,
  [-2]: `BAIL`
};
var slotFlagsText = {
  [1]: "STABLE",
  [2]: "DYNAMIC",
  [3]: "FORWARDED"
};
var specialBooleanAttrs = `itemscope,allowfullscreen,formnovalidate,ismap,nomodule,novalidate,readonly`;
var isBooleanAttr2 = /* @__PURE__ */ makeMap(specialBooleanAttrs + `,async,autofocus,autoplay,controls,default,defer,disabled,hidden,loop,open,required,reversed,scoped,seamless,checked,muted,multiple,selected`);
var EMPTY_OBJ =  true ? Object.freeze({}) : 0;
var EMPTY_ARR =  true ? Object.freeze([]) : 0;
var extend = Object.assign;
var hasOwnProperty = Object.prototype.hasOwnProperty;
var hasOwn = (val, key) => hasOwnProperty.call(val, key);
var isArray = Array.isArray;
var isMap = (val) => toTypeString(val) === "[object Map]";
var isString = (val) => typeof val === "string";
var isSymbol = (val) => typeof val === "symbol";
var isObject = (val) => val !== null && typeof val === "object";
var objectToString = Object.prototype.toString;
var toTypeString = (value) => objectToString.call(value);
var toRawType = (value) => {
  return toTypeString(value).slice(8, -1);
};
var isIntegerKey = (key) => isString(key) && key !== "NaN" && key[0] !== "-" && "" + parseInt(key, 10) === key;
var cacheStringFunction = (fn) => {
  const cache = Object.create(null);
  return (str) => {
    const hit = cache[str];
    return hit || (cache[str] = fn(str));
  };
};
var camelizeRE = /-(\w)/g;
var camelize = cacheStringFunction((str) => {
  return str.replace(camelizeRE, (_, c) => c ? c.toUpperCase() : "");
});
var hyphenateRE = /\B([A-Z])/g;
var hyphenate = cacheStringFunction((str) => str.replace(hyphenateRE, "-$1").toLowerCase());
var capitalize = cacheStringFunction((str) => str.charAt(0).toUpperCase() + str.slice(1));
var toHandlerKey = cacheStringFunction((str) => str ? `on${capitalize(str)}` : ``);
var hasChanged = (value, oldValue) => value !== oldValue && (value === value || oldValue === oldValue);

// node_modules/@vue/reactivity/dist/reactivity.esm-bundler.js
var targetMap = new WeakMap();
var effectStack = [];
var activeEffect;
var ITERATE_KEY = Symbol( true ? "iterate" : 0);
var MAP_KEY_ITERATE_KEY = Symbol( true ? "Map key iterate" : 0);
function isEffect(fn) {
  return fn && fn._isEffect === true;
}
function effect2(fn, options = EMPTY_OBJ) {
  if (isEffect(fn)) {
    fn = fn.raw;
  }
  const effect3 = createReactiveEffect(fn, options);
  if (!options.lazy) {
    effect3();
  }
  return effect3;
}
function stop(effect3) {
  if (effect3.active) {
    cleanup(effect3);
    if (effect3.options.onStop) {
      effect3.options.onStop();
    }
    effect3.active = false;
  }
}
var uid = 0;
function createReactiveEffect(fn, options) {
  const effect3 = function reactiveEffect() {
    if (!effect3.active) {
      return fn();
    }
    if (!effectStack.includes(effect3)) {
      cleanup(effect3);
      try {
        enableTracking();
        effectStack.push(effect3);
        activeEffect = effect3;
        return fn();
      } finally {
        effectStack.pop();
        resetTracking();
        activeEffect = effectStack[effectStack.length - 1];
      }
    }
  };
  effect3.id = uid++;
  effect3.allowRecurse = !!options.allowRecurse;
  effect3._isEffect = true;
  effect3.active = true;
  effect3.raw = fn;
  effect3.deps = [];
  effect3.options = options;
  return effect3;
}
function cleanup(effect3) {
  const {deps} = effect3;
  if (deps.length) {
    for (let i = 0; i < deps.length; i++) {
      deps[i].delete(effect3);
    }
    deps.length = 0;
  }
}
var shouldTrack = true;
var trackStack = [];
function pauseTracking() {
  trackStack.push(shouldTrack);
  shouldTrack = false;
}
function enableTracking() {
  trackStack.push(shouldTrack);
  shouldTrack = true;
}
function resetTracking() {
  const last = trackStack.pop();
  shouldTrack = last === void 0 ? true : last;
}
function track(target, type, key) {
  if (!shouldTrack || activeEffect === void 0) {
    return;
  }
  let depsMap = targetMap.get(target);
  if (!depsMap) {
    targetMap.set(target, depsMap = new Map());
  }
  let dep = depsMap.get(key);
  if (!dep) {
    depsMap.set(key, dep = new Set());
  }
  if (!dep.has(activeEffect)) {
    dep.add(activeEffect);
    activeEffect.deps.push(dep);
    if (activeEffect.options.onTrack) {
      activeEffect.options.onTrack({
        effect: activeEffect,
        target,
        type,
        key
      });
    }
  }
}
function trigger(target, type, key, newValue, oldValue, oldTarget) {
  const depsMap = targetMap.get(target);
  if (!depsMap) {
    return;
  }
  const effects = new Set();
  const add2 = (effectsToAdd) => {
    if (effectsToAdd) {
      effectsToAdd.forEach((effect3) => {
        if (effect3 !== activeEffect || effect3.allowRecurse) {
          effects.add(effect3);
        }
      });
    }
  };
  if (type === "clear") {
    depsMap.forEach(add2);
  } else if (key === "length" && isArray(target)) {
    depsMap.forEach((dep, key2) => {
      if (key2 === "length" || key2 >= newValue) {
        add2(dep);
      }
    });
  } else {
    if (key !== void 0) {
      add2(depsMap.get(key));
    }
    switch (type) {
      case "add":
        if (!isArray(target)) {
          add2(depsMap.get(ITERATE_KEY));
          if (isMap(target)) {
            add2(depsMap.get(MAP_KEY_ITERATE_KEY));
          }
        } else if (isIntegerKey(key)) {
          add2(depsMap.get("length"));
        }
        break;
      case "delete":
        if (!isArray(target)) {
          add2(depsMap.get(ITERATE_KEY));
          if (isMap(target)) {
            add2(depsMap.get(MAP_KEY_ITERATE_KEY));
          }
        }
        break;
      case "set":
        if (isMap(target)) {
          add2(depsMap.get(ITERATE_KEY));
        }
        break;
    }
  }
  const run = (effect3) => {
    if (effect3.options.onTrigger) {
      effect3.options.onTrigger({
        effect: effect3,
        target,
        key,
        type,
        newValue,
        oldValue,
        oldTarget
      });
    }
    if (effect3.options.scheduler) {
      effect3.options.scheduler(effect3);
    } else {
      effect3();
    }
  };
  effects.forEach(run);
}
var isNonTrackableKeys = /* @__PURE__ */ makeMap(`__proto__,__v_isRef,__isVue`);
var builtInSymbols = new Set(Object.getOwnPropertyNames(Symbol).map((key) => Symbol[key]).filter(isSymbol));
var get2 = /* @__PURE__ */ createGetter();
var shallowGet = /* @__PURE__ */ createGetter(false, true);
var readonlyGet = /* @__PURE__ */ createGetter(true);
var shallowReadonlyGet = /* @__PURE__ */ createGetter(true, true);
var arrayInstrumentations = {};
["includes", "indexOf", "lastIndexOf"].forEach((key) => {
  const method = Array.prototype[key];
  arrayInstrumentations[key] = function(...args) {
    const arr = toRaw(this);
    for (let i = 0, l = this.length; i < l; i++) {
      track(arr, "get", i + "");
    }
    const res = method.apply(arr, args);
    if (res === -1 || res === false) {
      return method.apply(arr, args.map(toRaw));
    } else {
      return res;
    }
  };
});
["push", "pop", "shift", "unshift", "splice"].forEach((key) => {
  const method = Array.prototype[key];
  arrayInstrumentations[key] = function(...args) {
    pauseTracking();
    const res = method.apply(this, args);
    resetTracking();
    return res;
  };
});
function createGetter(isReadonly = false, shallow = false) {
  return function get3(target, key, receiver) {
    if (key === "__v_isReactive") {
      return !isReadonly;
    } else if (key === "__v_isReadonly") {
      return isReadonly;
    } else if (key === "__v_raw" && receiver === (isReadonly ? shallow ? shallowReadonlyMap : readonlyMap : shallow ? shallowReactiveMap : reactiveMap).get(target)) {
      return target;
    }
    const targetIsArray = isArray(target);
    if (!isReadonly && targetIsArray && hasOwn(arrayInstrumentations, key)) {
      return Reflect.get(arrayInstrumentations, key, receiver);
    }
    const res = Reflect.get(target, key, receiver);
    if (isSymbol(key) ? builtInSymbols.has(key) : isNonTrackableKeys(key)) {
      return res;
    }
    if (!isReadonly) {
      track(target, "get", key);
    }
    if (shallow) {
      return res;
    }
    if (isRef(res)) {
      const shouldUnwrap = !targetIsArray || !isIntegerKey(key);
      return shouldUnwrap ? res.value : res;
    }
    if (isObject(res)) {
      return isReadonly ? readonly(res) : reactive2(res);
    }
    return res;
  };
}
var set2 = /* @__PURE__ */ createSetter();
var shallowSet = /* @__PURE__ */ createSetter(true);
function createSetter(shallow = false) {
  return function set3(target, key, value, receiver) {
    let oldValue = target[key];
    if (!shallow) {
      value = toRaw(value);
      oldValue = toRaw(oldValue);
      if (!isArray(target) && isRef(oldValue) && !isRef(value)) {
        oldValue.value = value;
        return true;
      }
    }
    const hadKey = isArray(target) && isIntegerKey(key) ? Number(key) < target.length : hasOwn(target, key);
    const result = Reflect.set(target, key, value, receiver);
    if (target === toRaw(receiver)) {
      if (!hadKey) {
        trigger(target, "add", key, value);
      } else if (hasChanged(value, oldValue)) {
        trigger(target, "set", key, value, oldValue);
      }
    }
    return result;
  };
}
function deleteProperty(target, key) {
  const hadKey = hasOwn(target, key);
  const oldValue = target[key];
  const result = Reflect.deleteProperty(target, key);
  if (result && hadKey) {
    trigger(target, "delete", key, void 0, oldValue);
  }
  return result;
}
function has(target, key) {
  const result = Reflect.has(target, key);
  if (!isSymbol(key) || !builtInSymbols.has(key)) {
    track(target, "has", key);
  }
  return result;
}
function ownKeys(target) {
  track(target, "iterate", isArray(target) ? "length" : ITERATE_KEY);
  return Reflect.ownKeys(target);
}
var mutableHandlers = {
  get: get2,
  set: set2,
  deleteProperty,
  has,
  ownKeys
};
var readonlyHandlers = {
  get: readonlyGet,
  set(target, key) {
    if (true) {
      console.warn(`Set operation on key "${String(key)}" failed: target is readonly.`, target);
    }
    return true;
  },
  deleteProperty(target, key) {
    if (true) {
      console.warn(`Delete operation on key "${String(key)}" failed: target is readonly.`, target);
    }
    return true;
  }
};
var shallowReactiveHandlers = extend({}, mutableHandlers, {
  get: shallowGet,
  set: shallowSet
});
var shallowReadonlyHandlers = extend({}, readonlyHandlers, {
  get: shallowReadonlyGet
});
var toReactive = (value) => isObject(value) ? reactive2(value) : value;
var toReadonly = (value) => isObject(value) ? readonly(value) : value;
var toShallow = (value) => value;
var getProto = (v) => Reflect.getPrototypeOf(v);
function get$1(target, key, isReadonly = false, isShallow = false) {
  target = target["__v_raw"];
  const rawTarget = toRaw(target);
  const rawKey = toRaw(key);
  if (key !== rawKey) {
    !isReadonly && track(rawTarget, "get", key);
  }
  !isReadonly && track(rawTarget, "get", rawKey);
  const {has: has2} = getProto(rawTarget);
  const wrap = isShallow ? toShallow : isReadonly ? toReadonly : toReactive;
  if (has2.call(rawTarget, key)) {
    return wrap(target.get(key));
  } else if (has2.call(rawTarget, rawKey)) {
    return wrap(target.get(rawKey));
  } else if (target !== rawTarget) {
    target.get(key);
  }
}
function has$1(key, isReadonly = false) {
  const target = this["__v_raw"];
  const rawTarget = toRaw(target);
  const rawKey = toRaw(key);
  if (key !== rawKey) {
    !isReadonly && track(rawTarget, "has", key);
  }
  !isReadonly && track(rawTarget, "has", rawKey);
  return key === rawKey ? target.has(key) : target.has(key) || target.has(rawKey);
}
function size(target, isReadonly = false) {
  target = target["__v_raw"];
  !isReadonly && track(toRaw(target), "iterate", ITERATE_KEY);
  return Reflect.get(target, "size", target);
}
function add(value) {
  value = toRaw(value);
  const target = toRaw(this);
  const proto = getProto(target);
  const hadKey = proto.has.call(target, value);
  if (!hadKey) {
    target.add(value);
    trigger(target, "add", value, value);
  }
  return this;
}
function set$1(key, value) {
  value = toRaw(value);
  const target = toRaw(this);
  const {has: has2, get: get3} = getProto(target);
  let hadKey = has2.call(target, key);
  if (!hadKey) {
    key = toRaw(key);
    hadKey = has2.call(target, key);
  } else if (true) {
    checkIdentityKeys(target, has2, key);
  }
  const oldValue = get3.call(target, key);
  target.set(key, value);
  if (!hadKey) {
    trigger(target, "add", key, value);
  } else if (hasChanged(value, oldValue)) {
    trigger(target, "set", key, value, oldValue);
  }
  return this;
}
function deleteEntry(key) {
  const target = toRaw(this);
  const {has: has2, get: get3} = getProto(target);
  let hadKey = has2.call(target, key);
  if (!hadKey) {
    key = toRaw(key);
    hadKey = has2.call(target, key);
  } else if (true) {
    checkIdentityKeys(target, has2, key);
  }
  const oldValue = get3 ? get3.call(target, key) : void 0;
  const result = target.delete(key);
  if (hadKey) {
    trigger(target, "delete", key, void 0, oldValue);
  }
  return result;
}
function clear() {
  const target = toRaw(this);
  const hadItems = target.size !== 0;
  const oldTarget =  true ? isMap(target) ? new Map(target) : new Set(target) : 0;
  const result = target.clear();
  if (hadItems) {
    trigger(target, "clear", void 0, void 0, oldTarget);
  }
  return result;
}
function createForEach(isReadonly, isShallow) {
  return function forEach(callback, thisArg) {
    const observed = this;
    const target = observed["__v_raw"];
    const rawTarget = toRaw(target);
    const wrap = isShallow ? toShallow : isReadonly ? toReadonly : toReactive;
    !isReadonly && track(rawTarget, "iterate", ITERATE_KEY);
    return target.forEach((value, key) => {
      return callback.call(thisArg, wrap(value), wrap(key), observed);
    });
  };
}
function createIterableMethod(method, isReadonly, isShallow) {
  return function(...args) {
    const target = this["__v_raw"];
    const rawTarget = toRaw(target);
    const targetIsMap = isMap(rawTarget);
    const isPair = method === "entries" || method === Symbol.iterator && targetIsMap;
    const isKeyOnly = method === "keys" && targetIsMap;
    const innerIterator = target[method](...args);
    const wrap = isShallow ? toShallow : isReadonly ? toReadonly : toReactive;
    !isReadonly && track(rawTarget, "iterate", isKeyOnly ? MAP_KEY_ITERATE_KEY : ITERATE_KEY);
    return {
      next() {
        const {value, done} = innerIterator.next();
        return done ? {value, done} : {
          value: isPair ? [wrap(value[0]), wrap(value[1])] : wrap(value),
          done
        };
      },
      [Symbol.iterator]() {
        return this;
      }
    };
  };
}
function createReadonlyMethod(type) {
  return function(...args) {
    if (true) {
      const key = args[0] ? `on key "${args[0]}" ` : ``;
      console.warn(`${capitalize(type)} operation ${key}failed: target is readonly.`, toRaw(this));
    }
    return type === "delete" ? false : this;
  };
}
var mutableInstrumentations = {
  get(key) {
    return get$1(this, key);
  },
  get size() {
    return size(this);
  },
  has: has$1,
  add,
  set: set$1,
  delete: deleteEntry,
  clear,
  forEach: createForEach(false, false)
};
var shallowInstrumentations = {
  get(key) {
    return get$1(this, key, false, true);
  },
  get size() {
    return size(this);
  },
  has: has$1,
  add,
  set: set$1,
  delete: deleteEntry,
  clear,
  forEach: createForEach(false, true)
};
var readonlyInstrumentations = {
  get(key) {
    return get$1(this, key, true);
  },
  get size() {
    return size(this, true);
  },
  has(key) {
    return has$1.call(this, key, true);
  },
  add: createReadonlyMethod("add"),
  set: createReadonlyMethod("set"),
  delete: createReadonlyMethod("delete"),
  clear: createReadonlyMethod("clear"),
  forEach: createForEach(true, false)
};
var shallowReadonlyInstrumentations = {
  get(key) {
    return get$1(this, key, true, true);
  },
  get size() {
    return size(this, true);
  },
  has(key) {
    return has$1.call(this, key, true);
  },
  add: createReadonlyMethod("add"),
  set: createReadonlyMethod("set"),
  delete: createReadonlyMethod("delete"),
  clear: createReadonlyMethod("clear"),
  forEach: createForEach(true, true)
};
var iteratorMethods = ["keys", "values", "entries", Symbol.iterator];
iteratorMethods.forEach((method) => {
  mutableInstrumentations[method] = createIterableMethod(method, false, false);
  readonlyInstrumentations[method] = createIterableMethod(method, true, false);
  shallowInstrumentations[method] = createIterableMethod(method, false, true);
  shallowReadonlyInstrumentations[method] = createIterableMethod(method, true, true);
});
function createInstrumentationGetter(isReadonly, shallow) {
  const instrumentations = shallow ? isReadonly ? shallowReadonlyInstrumentations : shallowInstrumentations : isReadonly ? readonlyInstrumentations : mutableInstrumentations;
  return (target, key, receiver) => {
    if (key === "__v_isReactive") {
      return !isReadonly;
    } else if (key === "__v_isReadonly") {
      return isReadonly;
    } else if (key === "__v_raw") {
      return target;
    }
    return Reflect.get(hasOwn(instrumentations, key) && key in target ? instrumentations : target, key, receiver);
  };
}
var mutableCollectionHandlers = {
  get: createInstrumentationGetter(false, false)
};
var shallowCollectionHandlers = {
  get: createInstrumentationGetter(false, true)
};
var readonlyCollectionHandlers = {
  get: createInstrumentationGetter(true, false)
};
var shallowReadonlyCollectionHandlers = {
  get: createInstrumentationGetter(true, true)
};
function checkIdentityKeys(target, has2, key) {
  const rawKey = toRaw(key);
  if (rawKey !== key && has2.call(target, rawKey)) {
    const type = toRawType(target);
    console.warn(`Reactive ${type} contains both the raw and reactive versions of the same object${type === `Map` ? ` as keys` : ``}, which can lead to inconsistencies. Avoid differentiating between the raw and reactive versions of an object and only use the reactive version if possible.`);
  }
}
var reactiveMap = new WeakMap();
var shallowReactiveMap = new WeakMap();
var readonlyMap = new WeakMap();
var shallowReadonlyMap = new WeakMap();
function targetTypeMap(rawType) {
  switch (rawType) {
    case "Object":
    case "Array":
      return 1;
    case "Map":
    case "Set":
    case "WeakMap":
    case "WeakSet":
      return 2;
    default:
      return 0;
  }
}
function getTargetType(value) {
  return value["__v_skip"] || !Object.isExtensible(value) ? 0 : targetTypeMap(toRawType(value));
}
function reactive2(target) {
  if (target && target["__v_isReadonly"]) {
    return target;
  }
  return createReactiveObject(target, false, mutableHandlers, mutableCollectionHandlers, reactiveMap);
}
function readonly(target) {
  return createReactiveObject(target, true, readonlyHandlers, readonlyCollectionHandlers, readonlyMap);
}
function createReactiveObject(target, isReadonly, baseHandlers, collectionHandlers, proxyMap) {
  if (!isObject(target)) {
    if (true) {
      console.warn(`value cannot be made reactive: ${String(target)}`);
    }
    return target;
  }
  if (target["__v_raw"] && !(isReadonly && target["__v_isReactive"])) {
    return target;
  }
  const existingProxy = proxyMap.get(target);
  if (existingProxy) {
    return existingProxy;
  }
  const targetType = getTargetType(target);
  if (targetType === 0) {
    return target;
  }
  const proxy = new Proxy(target, targetType === 2 ? collectionHandlers : baseHandlers);
  proxyMap.set(target, proxy);
  return proxy;
}
function toRaw(observed) {
  return observed && toRaw(observed["__v_raw"]) || observed;
}
function isRef(r) {
  return Boolean(r && r.__v_isRef === true);
}

// packages/alpinejs/src/magics/$nextTick.js
magic("nextTick", () => nextTick);

// packages/alpinejs/src/magics/$dispatch.js
magic("dispatch", (el) => dispatch.bind(dispatch, el));

// packages/alpinejs/src/magics/$watch.js
magic("watch", (el, {evaluateLater: evaluateLater2, effect: effect3}) => (key, callback) => {
  let evaluate2 = evaluateLater2(key);
  let firstTime = true;
  let oldValue;
  let effectReference = effect3(() => evaluate2((value) => {
    JSON.stringify(value);
    if (!firstTime) {
      queueMicrotask(() => {
        callback(value, oldValue);
        oldValue = value;
      });
    } else {
      oldValue = value;
    }
    firstTime = false;
  }));
  el._x_effects.delete(effectReference);
});

// packages/alpinejs/src/magics/$store.js
magic("store", getStores);

// packages/alpinejs/src/magics/$data.js
magic("data", (el) => scope(el));

// packages/alpinejs/src/magics/$root.js
magic("root", (el) => closestRoot(el));

// packages/alpinejs/src/magics/$refs.js
magic("refs", (el) => {
  if (el._x_refs_proxy)
    return el._x_refs_proxy;
  el._x_refs_proxy = mergeProxies(getArrayOfRefObject(el));
  return el._x_refs_proxy;
});
function getArrayOfRefObject(el) {
  let refObjects = [];
  let currentEl = el;
  while (currentEl) {
    if (currentEl._x_refs)
      refObjects.push(currentEl._x_refs);
    currentEl = currentEl.parentNode;
  }
  return refObjects;
}

// packages/alpinejs/src/ids.js
var globalIdMemo = {};
function findAndIncrementId(name) {
  if (!globalIdMemo[name])
    globalIdMemo[name] = 0;
  return ++globalIdMemo[name];
}
function closestIdRoot(el, name) {
  return findClosest(el, (element) => {
    if (element._x_ids && element._x_ids[name])
      return true;
  });
}
function setIdRoot(el, name) {
  if (!el._x_ids)
    el._x_ids = {};
  if (!el._x_ids[name])
    el._x_ids[name] = findAndIncrementId(name);
}

// packages/alpinejs/src/magics/$id.js
magic("id", (el) => (name, key = null) => {
  let root = closestIdRoot(el, name);
  let id = root ? root._x_ids[name] : findAndIncrementId(name);
  return key ? `${name}-${id}-${key}` : `${name}-${id}`;
});

// packages/alpinejs/src/magics/$el.js
magic("el", (el) => el);

// packages/alpinejs/src/magics/index.js
warnMissingPluginMagic("Focus", "focus", "focus");
warnMissingPluginMagic("Persist", "persist", "persist");
function warnMissingPluginMagic(name, magicName, slug) {
  magic(magicName, (el) => warn(`You can't use [$${directiveName}] without first installing the "${name}" plugin here: https://alpinejs.dev/plugins/${slug}`, el));
}

// packages/alpinejs/src/directives/x-modelable.js
directive("modelable", (el, {expression}, {effect: effect3, evaluateLater: evaluateLater2}) => {
  let func = evaluateLater2(expression);
  let innerGet = () => {
    let result;
    func((i) => result = i);
    return result;
  };
  let evaluateInnerSet = evaluateLater2(`${expression} = __placeholder`);
  let innerSet = (val) => evaluateInnerSet(() => {
  }, {scope: {__placeholder: val}});
  let initialValue = innerGet();
  innerSet(initialValue);
  queueMicrotask(() => {
    if (!el._x_model)
      return;
    el._x_removeModelListeners["default"]();
    let outerGet = el._x_model.get;
    let outerSet = el._x_model.set;
    effect3(() => innerSet(outerGet()));
    effect3(() => outerSet(innerGet()));
  });
});

// packages/alpinejs/src/directives/x-teleport.js
directive("teleport", (el, {expression}, {cleanup: cleanup2}) => {
  if (el.tagName.toLowerCase() !== "template")
    warn("x-teleport can only be used on a <template> tag", el);
  let target = document.querySelector(expression);
  if (!target)
    warn(`Cannot find x-teleport element for selector: "${expression}"`);
  let clone2 = el.content.cloneNode(true).firstElementChild;
  el._x_teleport = clone2;
  clone2._x_teleportBack = el;
  if (el._x_forwardEvents) {
    el._x_forwardEvents.forEach((eventName) => {
      clone2.addEventListener(eventName, (e) => {
        e.stopPropagation();
        el.dispatchEvent(new e.constructor(e.type, e));
      });
    });
  }
  addScopeToNode(clone2, {}, el);
  mutateDom(() => {
    target.appendChild(clone2);
    initTree(clone2);
    clone2._x_ignore = true;
  });
  cleanup2(() => clone2.remove());
});

// packages/alpinejs/src/directives/x-ignore.js
var handler = () => {
};
handler.inline = (el, {modifiers}, {cleanup: cleanup2}) => {
  modifiers.includes("self") ? el._x_ignoreSelf = true : el._x_ignore = true;
  cleanup2(() => {
    modifiers.includes("self") ? delete el._x_ignoreSelf : delete el._x_ignore;
  });
};
directive("ignore", handler);

// packages/alpinejs/src/directives/x-effect.js
directive("effect", (el, {expression}, {effect: effect3}) => effect3(evaluateLater(el, expression)));

// packages/alpinejs/src/utils/on.js
function on(el, event, modifiers, callback) {
  let listenerTarget = el;
  let handler3 = (e) => callback(e);
  let options = {};
  let wrapHandler = (callback2, wrapper) => (e) => wrapper(callback2, e);
  if (modifiers.includes("dot"))
    event = dotSyntax(event);
  if (modifiers.includes("camel"))
    event = camelCase2(event);
  if (modifiers.includes("passive"))
    options.passive = true;
  if (modifiers.includes("capture"))
    options.capture = true;
  if (modifiers.includes("window"))
    listenerTarget = window;
  if (modifiers.includes("document"))
    listenerTarget = document;
  if (modifiers.includes("prevent"))
    handler3 = wrapHandler(handler3, (next, e) => {
      e.preventDefault();
      next(e);
    });
  if (modifiers.includes("stop"))
    handler3 = wrapHandler(handler3, (next, e) => {
      e.stopPropagation();
      next(e);
    });
  if (modifiers.includes("self"))
    handler3 = wrapHandler(handler3, (next, e) => {
      e.target === el && next(e);
    });
  if (modifiers.includes("away") || modifiers.includes("outside")) {
    listenerTarget = document;
    handler3 = wrapHandler(handler3, (next, e) => {
      if (el.contains(e.target))
        return;
      if (e.target.isConnected === false)
        return;
      if (el.offsetWidth < 1 && el.offsetHeight < 1)
        return;
      if (el._x_isShown === false)
        return;
      next(e);
    });
  }
  if (modifiers.includes("once")) {
    handler3 = wrapHandler(handler3, (next, e) => {
      next(e);
      listenerTarget.removeEventListener(event, handler3, options);
    });
  }
  handler3 = wrapHandler(handler3, (next, e) => {
    if (isKeyEvent(event)) {
      if (isListeningForASpecificKeyThatHasntBeenPressed(e, modifiers)) {
        return;
      }
    }
    next(e);
  });
  if (modifiers.includes("debounce")) {
    let nextModifier = modifiers[modifiers.indexOf("debounce") + 1] || "invalid-wait";
    let wait = isNumeric(nextModifier.split("ms")[0]) ? Number(nextModifier.split("ms")[0]) : 250;
    handler3 = debounce(handler3, wait);
  }
  if (modifiers.includes("throttle")) {
    let nextModifier = modifiers[modifiers.indexOf("throttle") + 1] || "invalid-wait";
    let wait = isNumeric(nextModifier.split("ms")[0]) ? Number(nextModifier.split("ms")[0]) : 250;
    handler3 = throttle(handler3, wait);
  }
  listenerTarget.addEventListener(event, handler3, options);
  return () => {
    listenerTarget.removeEventListener(event, handler3, options);
  };
}
function dotSyntax(subject) {
  return subject.replace(/-/g, ".");
}
function camelCase2(subject) {
  return subject.toLowerCase().replace(/-(\w)/g, (match, char) => char.toUpperCase());
}
function isNumeric(subject) {
  return !Array.isArray(subject) && !isNaN(subject);
}
function kebabCase2(subject) {
  return subject.replace(/([a-z])([A-Z])/g, "$1-$2").replace(/[_\s]/, "-").toLowerCase();
}
function isKeyEvent(event) {
  return ["keydown", "keyup"].includes(event);
}
function isListeningForASpecificKeyThatHasntBeenPressed(e, modifiers) {
  let keyModifiers = modifiers.filter((i) => {
    return !["window", "document", "prevent", "stop", "once"].includes(i);
  });
  if (keyModifiers.includes("debounce")) {
    let debounceIndex = keyModifiers.indexOf("debounce");
    keyModifiers.splice(debounceIndex, isNumeric((keyModifiers[debounceIndex + 1] || "invalid-wait").split("ms")[0]) ? 2 : 1);
  }
  if (keyModifiers.length === 0)
    return false;
  if (keyModifiers.length === 1 && keyToModifiers(e.key).includes(keyModifiers[0]))
    return false;
  const systemKeyModifiers = ["ctrl", "shift", "alt", "meta", "cmd", "super"];
  const selectedSystemKeyModifiers = systemKeyModifiers.filter((modifier) => keyModifiers.includes(modifier));
  keyModifiers = keyModifiers.filter((i) => !selectedSystemKeyModifiers.includes(i));
  if (selectedSystemKeyModifiers.length > 0) {
    const activelyPressedKeyModifiers = selectedSystemKeyModifiers.filter((modifier) => {
      if (modifier === "cmd" || modifier === "super")
        modifier = "meta";
      return e[`${modifier}Key`];
    });
    if (activelyPressedKeyModifiers.length === selectedSystemKeyModifiers.length) {
      if (keyToModifiers(e.key).includes(keyModifiers[0]))
        return false;
    }
  }
  return true;
}
function keyToModifiers(key) {
  if (!key)
    return [];
  key = kebabCase2(key);
  let modifierToKeyMap = {
    ctrl: "control",
    slash: "/",
    space: "-",
    spacebar: "-",
    cmd: "meta",
    esc: "escape",
    up: "arrow-up",
    down: "arrow-down",
    left: "arrow-left",
    right: "arrow-right",
    period: ".",
    equal: "="
  };
  modifierToKeyMap[key] = key;
  return Object.keys(modifierToKeyMap).map((modifier) => {
    if (modifierToKeyMap[modifier] === key)
      return modifier;
  }).filter((modifier) => modifier);
}

// packages/alpinejs/src/directives/x-model.js
directive("model", (el, {modifiers, expression}, {effect: effect3, cleanup: cleanup2}) => {
  let evaluate2 = evaluateLater(el, expression);
  let assignmentExpression = `${expression} = rightSideOfExpression($event, ${expression})`;
  let evaluateAssignment = evaluateLater(el, assignmentExpression);
  var event = el.tagName.toLowerCase() === "select" || ["checkbox", "radio"].includes(el.type) || modifiers.includes("lazy") ? "change" : "input";
  let assigmentFunction = generateAssignmentFunction(el, modifiers, expression);
  let removeListener = on(el, event, modifiers, (e) => {
    evaluateAssignment(() => {
    }, {scope: {
      $event: e,
      rightSideOfExpression: assigmentFunction
    }});
  });
  if (!el._x_removeModelListeners)
    el._x_removeModelListeners = {};
  el._x_removeModelListeners["default"] = removeListener;
  cleanup2(() => el._x_removeModelListeners["default"]());
  let evaluateSetModel = evaluateLater(el, `${expression} = __placeholder`);
  el._x_model = {
    get() {
      let result;
      evaluate2((value) => result = value);
      return result;
    },
    set(value) {
      evaluateSetModel(() => {
      }, {scope: {__placeholder: value}});
    }
  };
  el._x_forceModelUpdate = () => {
    evaluate2((value) => {
      if (value === void 0 && expression.match(/\./))
        value = "";
      window.fromModel = true;
      mutateDom(() => bind(el, "value", value));
      delete window.fromModel;
    });
  };
  effect3(() => {
    if (modifiers.includes("unintrusive") && document.activeElement.isSameNode(el))
      return;
    el._x_forceModelUpdate();
  });
});
function generateAssignmentFunction(el, modifiers, expression) {
  if (el.type === "radio") {
    mutateDom(() => {
      if (!el.hasAttribute("name"))
        el.setAttribute("name", expression);
    });
  }
  return (event, currentValue) => {
    return mutateDom(() => {
      if (event instanceof CustomEvent && event.detail !== void 0) {
        return event.detail || event.target.value;
      } else if (el.type === "checkbox") {
        if (Array.isArray(currentValue)) {
          let newValue = modifiers.includes("number") ? safeParseNumber(event.target.value) : event.target.value;
          return event.target.checked ? currentValue.concat([newValue]) : currentValue.filter((el2) => !checkedAttrLooseCompare2(el2, newValue));
        } else {
          return event.target.checked;
        }
      } else if (el.tagName.toLowerCase() === "select" && el.multiple) {
        return modifiers.includes("number") ? Array.from(event.target.selectedOptions).map((option) => {
          let rawValue = option.value || option.text;
          return safeParseNumber(rawValue);
        }) : Array.from(event.target.selectedOptions).map((option) => {
          return option.value || option.text;
        });
      } else {
        let rawValue = event.target.value;
        return modifiers.includes("number") ? safeParseNumber(rawValue) : modifiers.includes("trim") ? rawValue.trim() : rawValue;
      }
    });
  };
}
function safeParseNumber(rawValue) {
  let number = rawValue ? parseFloat(rawValue) : null;
  return isNumeric2(number) ? number : rawValue;
}
function checkedAttrLooseCompare2(valueA, valueB) {
  return valueA == valueB;
}
function isNumeric2(subject) {
  return !Array.isArray(subject) && !isNaN(subject);
}

// packages/alpinejs/src/directives/x-cloak.js
directive("cloak", (el) => queueMicrotask(() => mutateDom(() => el.removeAttribute(prefix("cloak")))));

// packages/alpinejs/src/directives/x-init.js
addInitSelector(() => `[${prefix("init")}]`);
directive("init", skipDuringClone((el, {expression}, {evaluate: evaluate2}) => {
  if (typeof expression === "string") {
    return !!expression.trim() && evaluate2(expression, {}, false);
  }
  return evaluate2(expression, {}, false);
}));

// packages/alpinejs/src/directives/x-text.js
directive("text", (el, {expression}, {effect: effect3, evaluateLater: evaluateLater2}) => {
  let evaluate2 = evaluateLater2(expression);
  effect3(() => {
    evaluate2((value) => {
      mutateDom(() => {
        el.textContent = value;
      });
    });
  });
});

// packages/alpinejs/src/directives/x-html.js
directive("html", (el, {expression}, {effect: effect3, evaluateLater: evaluateLater2}) => {
  let evaluate2 = evaluateLater2(expression);
  effect3(() => {
    evaluate2((value) => {
      mutateDom(() => {
        el.innerHTML = value;
        el._x_ignoreSelf = true;
        initTree(el);
        delete el._x_ignoreSelf;
      });
    });
  });
});

// packages/alpinejs/src/directives/x-bind.js
mapAttributes(startingWith(":", into(prefix("bind:"))));
directive("bind", (el, {value, modifiers, expression, original}, {effect: effect3}) => {
  if (!value) {
    let bindingProviders = {};
    injectBindingProviders(bindingProviders);
    let getBindings = evaluateLater(el, expression);
    getBindings((bindings) => {
      applyBindingsObject(el, bindings, original);
    }, {scope: bindingProviders});
    return;
  }
  if (value === "key")
    return storeKeyForXFor(el, expression);
  let evaluate2 = evaluateLater(el, expression);
  effect3(() => evaluate2((result) => {
    if (result === void 0 && typeof expression === "string" && expression.match(/\./)) {
      result = "";
    }
    mutateDom(() => bind(el, value, result, modifiers));
  }));
});
function storeKeyForXFor(el, expression) {
  el._x_keyExpression = expression;
}

// packages/alpinejs/src/directives/x-data.js
addRootSelector(() => `[${prefix("data")}]`);
directive("data", skipDuringClone((el, {expression}, {cleanup: cleanup2}) => {
  expression = expression === "" ? "{}" : expression;
  let magicContext = {};
  injectMagics(magicContext, el);
  let dataProviderContext = {};
  injectDataProviders(dataProviderContext, magicContext);
  let data2 = evaluate(el, expression, {scope: dataProviderContext});
  if (data2 === void 0)
    data2 = {};
  injectMagics(data2, el);
  let reactiveData = reactive(data2);
  initInterceptors(reactiveData);
  let undo = addScopeToNode(el, reactiveData);
  reactiveData["init"] && evaluate(el, reactiveData["init"]);
  cleanup2(() => {
    reactiveData["destroy"] && evaluate(el, reactiveData["destroy"]);
    undo();
  });
}));

// packages/alpinejs/src/directives/x-show.js
directive("show", (el, {modifiers, expression}, {effect: effect3}) => {
  let evaluate2 = evaluateLater(el, expression);
  if (!el._x_doHide)
    el._x_doHide = () => {
      mutateDom(() => {
        el.style.setProperty("display", "none", modifiers.includes("important") ? "important" : void 0);
      });
    };
  if (!el._x_doShow)
    el._x_doShow = () => {
      mutateDom(() => {
        if (el.style.length === 1 && el.style.display === "none") {
          el.removeAttribute("style");
        } else {
          el.style.removeProperty("display");
        }
      });
    };
  let hide = () => {
    el._x_doHide();
    el._x_isShown = false;
  };
  let show = () => {
    el._x_doShow();
    el._x_isShown = true;
  };
  let clickAwayCompatibleShow = () => setTimeout(show);
  let toggle = once((value) => value ? show() : hide(), (value) => {
    if (typeof el._x_toggleAndCascadeWithTransitions === "function") {
      el._x_toggleAndCascadeWithTransitions(el, value, show, hide);
    } else {
      value ? clickAwayCompatibleShow() : hide();
    }
  });
  let oldValue;
  let firstTime = true;
  effect3(() => evaluate2((value) => {
    if (!firstTime && value === oldValue)
      return;
    if (modifiers.includes("immediate"))
      value ? clickAwayCompatibleShow() : hide();
    toggle(value);
    oldValue = value;
    firstTime = false;
  }));
});

// packages/alpinejs/src/directives/x-for.js
directive("for", (el, {expression}, {effect: effect3, cleanup: cleanup2}) => {
  let iteratorNames = parseForExpression(expression);
  let evaluateItems = evaluateLater(el, iteratorNames.items);
  let evaluateKey = evaluateLater(el, el._x_keyExpression || "index");
  el._x_prevKeys = [];
  el._x_lookup = {};
  effect3(() => loop(el, iteratorNames, evaluateItems, evaluateKey));
  cleanup2(() => {
    Object.values(el._x_lookup).forEach((el2) => el2.remove());
    delete el._x_prevKeys;
    delete el._x_lookup;
  });
});
function loop(el, iteratorNames, evaluateItems, evaluateKey) {
  let isObject2 = (i) => typeof i === "object" && !Array.isArray(i);
  let templateEl = el;
  evaluateItems((items) => {
    if (isNumeric3(items) && items >= 0) {
      items = Array.from(Array(items).keys(), (i) => i + 1);
    }
    if (items === void 0)
      items = [];
    let lookup = el._x_lookup;
    let prevKeys = el._x_prevKeys;
    let scopes = [];
    let keys = [];
    if (isObject2(items)) {
      items = Object.entries(items).map(([key, value]) => {
        let scope2 = getIterationScopeVariables(iteratorNames, value, key, items);
        evaluateKey((value2) => keys.push(value2), {scope: {index: key, ...scope2}});
        scopes.push(scope2);
      });
    } else {
      for (let i = 0; i < items.length; i++) {
        let scope2 = getIterationScopeVariables(iteratorNames, items[i], i, items);
        evaluateKey((value) => keys.push(value), {scope: {index: i, ...scope2}});
        scopes.push(scope2);
      }
    }
    let adds = [];
    let moves = [];
    let removes = [];
    let sames = [];
    for (let i = 0; i < prevKeys.length; i++) {
      let key = prevKeys[i];
      if (keys.indexOf(key) === -1)
        removes.push(key);
    }
    prevKeys = prevKeys.filter((key) => !removes.includes(key));
    let lastKey = "template";
    for (let i = 0; i < keys.length; i++) {
      let key = keys[i];
      let prevIndex = prevKeys.indexOf(key);
      if (prevIndex === -1) {
        prevKeys.splice(i, 0, key);
        adds.push([lastKey, i]);
      } else if (prevIndex !== i) {
        let keyInSpot = prevKeys.splice(i, 1)[0];
        let keyForSpot = prevKeys.splice(prevIndex - 1, 1)[0];
        prevKeys.splice(i, 0, keyForSpot);
        prevKeys.splice(prevIndex, 0, keyInSpot);
        moves.push([keyInSpot, keyForSpot]);
      } else {
        sames.push(key);
      }
      lastKey = key;
    }
    for (let i = 0; i < removes.length; i++) {
      let key = removes[i];
      if (!!lookup[key]._x_effects) {
        lookup[key]._x_effects.forEach(dequeueJob);
      }
      lookup[key].remove();
      lookup[key] = null;
      delete lookup[key];
    }
    for (let i = 0; i < moves.length; i++) {
      let [keyInSpot, keyForSpot] = moves[i];
      let elInSpot = lookup[keyInSpot];
      let elForSpot = lookup[keyForSpot];
      let marker = document.createElement("div");
      mutateDom(() => {
        elForSpot.after(marker);
        elInSpot.after(elForSpot);
        elForSpot._x_currentIfEl && elForSpot.after(elForSpot._x_currentIfEl);
        marker.before(elInSpot);
        elInSpot._x_currentIfEl && elInSpot.after(elInSpot._x_currentIfEl);
        marker.remove();
      });
      refreshScope(elForSpot, scopes[keys.indexOf(keyForSpot)]);
    }
    for (let i = 0; i < adds.length; i++) {
      let [lastKey2, index] = adds[i];
      let lastEl = lastKey2 === "template" ? templateEl : lookup[lastKey2];
      if (lastEl._x_currentIfEl)
        lastEl = lastEl._x_currentIfEl;
      let scope2 = scopes[index];
      let key = keys[index];
      let clone2 = document.importNode(templateEl.content, true).firstElementChild;
      addScopeToNode(clone2, reactive(scope2), templateEl);
      mutateDom(() => {
        lastEl.after(clone2);
        initTree(clone2);
      });
      if (typeof key === "object") {
        warn("x-for key cannot be an object, it must be a string or an integer", templateEl);
      }
      lookup[key] = clone2;
    }
    for (let i = 0; i < sames.length; i++) {
      refreshScope(lookup[sames[i]], scopes[keys.indexOf(sames[i])]);
    }
    templateEl._x_prevKeys = keys;
  });
}
function parseForExpression(expression) {
  let forIteratorRE = /,([^,\}\]]*)(?:,([^,\}\]]*))?$/;
  let stripParensRE = /^\s*\(|\)\s*$/g;
  let forAliasRE = /([\s\S]*?)\s+(?:in|of)\s+([\s\S]*)/;
  let inMatch = expression.match(forAliasRE);
  if (!inMatch)
    return;
  let res = {};
  res.items = inMatch[2].trim();
  let item = inMatch[1].replace(stripParensRE, "").trim();
  let iteratorMatch = item.match(forIteratorRE);
  if (iteratorMatch) {
    res.item = item.replace(forIteratorRE, "").trim();
    res.index = iteratorMatch[1].trim();
    if (iteratorMatch[2]) {
      res.collection = iteratorMatch[2].trim();
    }
  } else {
    res.item = item;
  }
  return res;
}
function getIterationScopeVariables(iteratorNames, item, index, items) {
  let scopeVariables = {};
  if (/^\[.*\]$/.test(iteratorNames.item) && Array.isArray(item)) {
    let names = iteratorNames.item.replace("[", "").replace("]", "").split(",").map((i) => i.trim());
    names.forEach((name, i) => {
      scopeVariables[name] = item[i];
    });
  } else if (/^\{.*\}$/.test(iteratorNames.item) && !Array.isArray(item) && typeof item === "object") {
    let names = iteratorNames.item.replace("{", "").replace("}", "").split(",").map((i) => i.trim());
    names.forEach((name) => {
      scopeVariables[name] = item[name];
    });
  } else {
    scopeVariables[iteratorNames.item] = item;
  }
  if (iteratorNames.index)
    scopeVariables[iteratorNames.index] = index;
  if (iteratorNames.collection)
    scopeVariables[iteratorNames.collection] = items;
  return scopeVariables;
}
function isNumeric3(subject) {
  return !Array.isArray(subject) && !isNaN(subject);
}

// packages/alpinejs/src/directives/x-ref.js
function handler2() {
}
handler2.inline = (el, {expression}, {cleanup: cleanup2}) => {
  let root = closestRoot(el);
  if (!root._x_refs)
    root._x_refs = {};
  root._x_refs[expression] = el;
  cleanup2(() => delete root._x_refs[expression]);
};
directive("ref", handler2);

// packages/alpinejs/src/directives/x-if.js
directive("if", (el, {expression}, {effect: effect3, cleanup: cleanup2}) => {
  let evaluate2 = evaluateLater(el, expression);
  let show = () => {
    if (el._x_currentIfEl)
      return el._x_currentIfEl;
    let clone2 = el.content.cloneNode(true).firstElementChild;
    addScopeToNode(clone2, {}, el);
    mutateDom(() => {
      el.after(clone2);
      initTree(clone2);
    });
    el._x_currentIfEl = clone2;
    el._x_undoIf = () => {
      walk(clone2, (node) => {
        if (!!node._x_effects) {
          node._x_effects.forEach(dequeueJob);
        }
      });
      clone2.remove();
      delete el._x_currentIfEl;
    };
    return clone2;
  };
  let hide = () => {
    if (!el._x_undoIf)
      return;
    el._x_undoIf();
    delete el._x_undoIf;
  };
  effect3(() => evaluate2((value) => {
    value ? show() : hide();
  }));
  cleanup2(() => el._x_undoIf && el._x_undoIf());
});

// packages/alpinejs/src/directives/x-id.js
directive("id", (el, {expression}, {evaluate: evaluate2}) => {
  let names = evaluate2(expression);
  names.forEach((name) => setIdRoot(el, name));
});

// packages/alpinejs/src/directives/x-on.js
mapAttributes(startingWith("@", into(prefix("on:"))));
directive("on", skipDuringClone((el, {value, modifiers, expression}, {cleanup: cleanup2}) => {
  let evaluate2 = expression ? evaluateLater(el, expression) : () => {
  };
  if (el.tagName.toLowerCase() === "template") {
    if (!el._x_forwardEvents)
      el._x_forwardEvents = [];
    if (!el._x_forwardEvents.includes(value))
      el._x_forwardEvents.push(value);
  }
  let removeListener = on(el, value, modifiers, (e) => {
    evaluate2(() => {
    }, {scope: {$event: e}, params: [e]});
  });
  cleanup2(() => removeListener());
}));

// packages/alpinejs/src/directives/index.js
warnMissingPluginDirective("Collapse", "collapse", "collapse");
warnMissingPluginDirective("Intersect", "intersect", "intersect");
warnMissingPluginDirective("Focus", "trap", "focus");
warnMissingPluginDirective("Mask", "mask", "mask");
function warnMissingPluginDirective(name, directiveName2, slug) {
  directive(directiveName2, (el) => warn(`You can't use [x-${directiveName2}] without first installing the "${name}" plugin here: https://alpinejs.dev/plugins/${slug}`, el));
}

// packages/alpinejs/src/index.js
alpine_default.setEvaluator(normalEvaluator);
alpine_default.setReactivityEngine({reactive: reactive2, effect: effect2, release: stop, raw: toRaw});
var src_default = alpine_default;

// packages/alpinejs/builds/module.js
var module_default = src_default;



/***/ }),

/***/ "./js/ajaxForm.ts":
/*!************************!*\
  !*** ./js/ajaxForm.ts ***!
  \************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () {
    document.addEventListener("submit", function (event) {
        var _a;
        var target = event.target;
        if (!target || !target.classList.contains("js-ajax-form-submit")) {
            return;
        }
        event.preventDefault();
        var data = new FormData(target);
        data.append("isAjax", "true");
        if ((_a = event.submitter) === null || _a === void 0 ? void 0 : _a.name) {
            var buttonSubmitter = event.submitter;
            data.append(buttonSubmitter.name, buttonSubmitter.value);
        }
        fetch(target.action, {
            method: 'POST',
            body: data
        }).then(function (res) { return res.json(); })
            .then(function (res) {
            target.reset();
            if (Array.isArray(res)) {
                res.forEach(function (item) {
                    updateDomContent(item);
                });
            }
            else {
                updateDomContent(res);
            }
        });
        return false;
    });
});
function updateDomContent(res) {
    var element = document.getElementById(res.id);
    if (res.isInner) {
        element.innerHTML = res.content;
    }
    else {
        element.outerHTML = res.content;
    }
    // @ts-ignore
    htmx.process(element);
}


/***/ }),

/***/ "./js/anchorHover.ts":
/*!***************************!*\
  !*** ./js/anchorHover.ts ***!
  \***************************/
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {


exports.__esModule = true;
var dom_1 = __webpack_require__(/*! @floating-ui/dom */ "./node_modules/@floating-ui/dom/dist/floating-ui.dom.esm.js");
exports["default"] = (function () {
    document
        .querySelectorAll(".anchor-source")
        .forEach(function (element) {
        var targetId = element.getAttribute("data-anchor-target-id");
        var target = document.querySelector("#" + targetId);
        if (!target) {
            return;
        }
        element.addEventListener("click", open);
        function open(event) {
            event.stopPropagation();
            target.classList.remove("hidden");
            update(element, target);
            element.removeEventListener("click", open);
            element.addEventListener("click", close);
            window.addEventListener("click", tryClose);
        }
        function tryClose(event) {
            var clickTarget = event.target;
            if (!clickTarget.closest(target.id)) {
                close();
            }
        }
        function close() {
            target.classList.add("hidden");
            element.addEventListener("click", open);
            element.removeEventListener("click", close);
            window.removeEventListener("click", tryClose);
        }
    });
    function update(buttonElem, targetElem) {
        var arrowId = buttonElem.getAttribute("data-anchor-arrow-id");
        var arrowEl = document.querySelector("#" + arrowId);
        (0, dom_1.computePosition)(buttonElem, targetElem, {
            placement: "top-start",
            middleware: [(0, dom_1.offset)(6), (0, dom_1.flip)(), (0, dom_1.shift)(), (0, dom_1.arrow)({ element: arrowEl })]
        }).then(function (_a) {
            var _b;
            var x = _a.x, y = _a.y, placement = _a.placement, middlewareData = _a.middlewareData;
            Object.assign(targetElem.style, {
                left: "".concat(x, "px"),
                top: "".concat(y, "px")
            });
            // Accessing the data
            var _c = middlewareData.arrow, arrowX = _c.x, arrowY = _c.y;
            var staticSide = {
                top: "bottom",
                right: "left",
                bottom: "top",
                left: "right"
            }[placement.split("-")[0]];
            Object.assign(arrowEl.style, (_b = {
                    left: arrowX != null ? "".concat(arrowX, "px") : "",
                    top: arrowY != null ? "".concat(arrowY, "px") : "",
                    right: "",
                    bottom: ""
                },
                _b[staticSide] = "-4px",
                _b));
        });
    }
});


/***/ }),

/***/ "./js/classHover.ts":
/*!**************************!*\
  !*** ./js/classHover.ts ***!
  \**************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () {
    document.querySelectorAll(".js-class-hover").forEach(function (element) {
        element.addEventListener("mouseenter", function (event) {
            var targetElement = event.currentTarget;
            var classToAdd = targetElement.getAttribute("js-class-hover");
            targetElement.classList.add(classToAdd);
        });
        element.addEventListener("mouseleave", function (event) {
            var targetElement = event.currentTarget;
            var classToAdd = targetElement.getAttribute("js-class-hover");
            targetElement.classList.remove(classToAdd);
        });
    });
});


/***/ }),

/***/ "./js/components/importModal.ts":
/*!**************************************!*\
  !*** ./js/components/importModal.ts ***!
  \**************************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () { return ({
    step: 0,
    loading: false,
    file: undefined,
    errorMessage: undefined,
    submitForm: function (event) {
        var _this = this;
        var target = event.target;
        event.preventDefault();
        var data = new FormData(target);
        this.loading = true;
        this.errorMessage = undefined;
        fetch(target.action, {
            method: 'POST',
            body: data
        }).then(function (resp) {
            if (resp.ok) {
                target.reset();
                _this.loading = false;
                _this.step = 1;
            }
            else {
                _this.loading = false;
                resp.text().then(function (text) {
                    _this.errorMessage = text;
                });
            }
        }, function (error) {
            console.log(error);
            _this.loading = false;
        });
        return false;
    }
}); });


/***/ }),

/***/ "./js/components/listManager.ts":
/*!**************************************!*\
  !*** ./js/components/listManager.ts ***!
  \**************************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () {
    document.querySelectorAll("[js-list]").forEach(function (elem) {
        var currentValue = elem.getAttribute("js-list-value") == "True" ? true : false;
        var list = elem.getAttribute("js-list");
        var setId = elem.getAttribute("js-set");
        elem.addEventListener("click", function () {
            fetch("/umbraco/api/collection/updateSetInList", {
                body: JSON.stringify({
                    setId: setId,
                    listName: list,
                    value: currentValue
                }),
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                }
            })
                .then(function (resp) { return resp.json(); })
                .then(function (resp) {
                elem.innerText = resp.text;
                currentValue = !currentValue;
            });
        });
    });
});


/***/ }),

/***/ "./js/copyToClipboardButton.ts":
/*!*************************************!*\
  !*** ./js/copyToClipboardButton.ts ***!
  \*************************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () {
    document.querySelectorAll(".js-copy-clipboard-button").forEach(function (element) {
        element.addEventListener("click", function (event) {
            var targetElement = event.currentTarget;
            var url = targetElement.href;
            fetch(url)
                .then(function (response) { return response.text(); })
                .then(function (response) {
                navigator.clipboard.writeText(response);
                targetElement.getElementsByTagName("p")[0].textContent = "Copied to clipboard";
            });
            event.preventDefault();
        });
    });
});


/***/ }),

/***/ "./js/cursorImage.ts":
/*!***************************!*\
  !*** ./js/cursorImage.ts ***!
  \***************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () {
    var cursorImageElem = document.querySelector("#cursor-image");
    if (!cursorImageElem) {
        return;
    }
    var cursorImageElemRect = cursorImageElem.getBoundingClientRect();
    var currentSourceCursorImage = null;
    window.addEventListener("mouseover", function (event) {
        var sourceCursorImage = event.target.closest('.cursor-source');
        if (!sourceCursorImage) {
            deselectCurrentSource();
            return;
        }
        currentSourceCursorImage = sourceCursorImage;
        cursorImageElem.style.backgroundImage = "url(".concat(sourceCursorImage.getAttribute("data-cursor-image"), ")");
        window.addEventListener("mousemove", updateCursorImageLocation);
    });
    function deselectCurrentSource() {
        if (currentSourceCursorImage) {
            window.removeEventListener("mousemove", updateCursorImageLocation);
            cursorImageElem.style.backgroundImage = null;
            currentSourceCursorImage = null;
        }
    }
    function updateCursorImageLocation(event) {
        if (cursorImageElemRect.height + event.clientY >= window.innerHeight) {
            cursorImageElem.style.top = "".concat(event.pageY - cursorImageElemRect.height, "px");
        }
        else {
            cursorImageElem.style.top = "".concat(event.pageY - 20, "px");
        }
        if (cursorImageElemRect.width + event.clientX + 40 >= window.innerWidth) {
            cursorImageElem.style.left = "".concat(event.pageX - cursorImageElemRect.width - 20, "px");
        }
        else {
            cursorImageElem.style.left = "".concat(event.pageX + 20, "px");
        }
    }
});


/***/ }),

/***/ "./js/deckCollection.ts":
/*!******************************!*\
  !*** ./js/deckCollection.ts ***!
  \******************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
function loadDeckCollectionProgress() {
    var elements = document.querySelectorAll('[data-deck-collection]');
    if (elements.length === 0)
        return;
    var deckIds = Array.from(elements)
        .map(function (el) { return el.getAttribute('data-deck-collection'); })
        .filter(function (id) { return id !== null; });
    var queryString = deckIds.map(function (id) { return "deckIds=".concat(id); }).join('&');
    fetch("/umbraco/api/collection/getdecksprogress?".concat(queryString))
        .then(function (res) { return res.json(); })
        .then(function (data) {
        data.forEach(function (item) {
            var el = document.querySelector("[data-deck-collection=\"".concat(item.deckId, "\"]"));
            if (el) {
                el.textContent = "Collection: ".concat(item.progress.toFixed(2), "%");
            }
        });
    })["catch"](function () { });
}
exports["default"] = (function () {
    document.addEventListener('DOMContentLoaded', loadDeckCollectionProgress);
    window.addEventListener('deck-overview-updated', loadDeckCollectionProgress);
});


/***/ }),

/***/ "./js/filteredOverview.ts":
/*!********************************!*\
  !*** ./js/filteredOverview.ts ***!
  \********************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function (config) { return ({
    filters: [],
    init: function () {
        var _this = this;
        window.addEventListener("SetFilter", function (event) {
            _this.updateFilterByValues(event.detail.name, event.detail.value, event.detail.option);
        });
        window.addEventListener("ResetFilters", function () { return _this.resetFilters(); });
    },
    registerFilter: function (element) {
        var filter = this.filters.find(function (filter) { return filter.key === element.name; });
        if (!filter) {
            filter = {
                key: element.name,
                filterItems: []
            };
            this.filters.push(filter);
        }
        filter.filterItems.push({
            value: element.value,
            option: element.checked ? ItemOption.Include : ItemOption.None,
            element: element
        });
    },
    updateFilter: function (event) {
        this.updateFilterByTarget(event.target);
    },
    updateFilterByTarget: function (target) {
        var filterName = target.name;
        var filterValue = target.value;
        var filterCheck = target.checked;
        this.updateFilterByValues(filterName, filterValue, filterCheck ? ItemOption.Include : ItemOption.None);
    },
    updateFilterByValues: function (name, value, option) {
        var filter = this.filters.find(function (item) { return item.key === name; });
        if (!filter) {
            return;
        }
        var filterItem = filter.filterItems.find(function (item) { return item.value === value; });
        if (!filterItem) {
            return;
        }
        if (filterItem.option === option) {
            return;
        }
        filterItem.option = option;
        if (filterItem.element) {
            if (filterItem.option === ItemOption.Include) {
                filterItem.element.checked = true;
            }
            else if (filterItem.option === ItemOption.None) {
                filterItem.element.checked = false;
            }
        }
        this.updateUrl();
    },
    addFilterValue: function (name, value) {
        var filter = this.filters.find(function (filter) { return filter.key === name; });
        if (!filter) {
            filter = {
                key: name,
                filterItems: []
            };
            this.filters.push(filter);
        }
        filter.filterItems.push({
            value: value,
            option: ItemOption.Include
        });
    },
    resetFilters: function () {
        var _this = this;
        this.filters.forEach(function (filter) {
            filter.filterItems.forEach(function (item) {
                if (item.option === ItemOption.None) {
                    return false;
                }
                _this.removeFilter(item);
            });
        });
    },
    removeFilter: function (filterItem) {
        filterItem.option = ItemOption.None;
        if (filterItem.element) {
            filterItem.element.checked = false;
        }
        this.updateUrl();
    },
    updateUrl: function () {
        if (!config.changeUrl) {
            return;
        }
        var url = new URL(window.location.href.split('?')[0]);
        this.filters.forEach(function (filter) {
            var checkedItems = filter.filterItems.filter(function (item) { return item.option === ItemOption.Include; });
            if (checkedItems.length === 0) {
                url.searchParams["delete"](filter.key);
                return;
            }
            checkedItems.forEach(function (item) {
                url.searchParams.append(filter.key, item.value);
            });
        });
        history.replaceState({}, null, url);
    }
}); });
var ItemOption;
(function (ItemOption) {
    ItemOption[ItemOption["None"] = 0] = "None";
    ItemOption[ItemOption["Include"] = 1] = "Include";
    ItemOption[ItemOption["Exclude"] = 2] = "Exclude";
})(ItemOption || (ItemOption = {}));


/***/ }),

/***/ "./js/modalButton.ts":
/*!***************************!*\
  !*** ./js/modalButton.ts ***!
  \***************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () {
    document.addEventListener("click", function (ev) {
        var target = ev.target.closest(".js-open-modal");
        if (!target) {
            return;
        }
        var modalId = target.getAttribute("js-modal");
        document.getElementById(modalId).showModal();
        ev.preventDefault();
    });
});


/***/ }),

/***/ "./js/serverSideOverview.ts":
/*!**********************************!*\
  !*** ./js/serverSideOverview.ts ***!
  \**********************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function (config) { return ({
    filters: [],
    loading: false,
    pageNumber: undefined,
    init: function () {
        var _this = this;
        window.addEventListener("SetFilter", function (event) {
            _this.updateFilterByValues(event.detail.name, event.detail.value, event.detail.option);
        });
        window.addEventListener("ResetFilters", function () { return _this.resetFilters(); });
        this.$refs.submitForm.addEventListener("submit", function (event) {
            event.preventDefault();
            _this.updateOverview();
        });
    },
    registerFilter: function (element, name) {
        var filter = this.filters.find(function (filter) { return filter.key === name; });
        if (!filter) {
            filter = {
                key: name,
                filterItems: [],
                elementLess: false
            };
            this.filters.push(filter);
        }
        filter.filterItems.push({
            value: element.value,
            label: element.value,
            option: element.checked ? ItemOption.Include : ItemOption.None,
            element: element
        });
    },
    updateFilter: function (event, name) {
        this.updateFilterByTarget(event.target, name);
    },
    updateFilterByTarget: function (target, name) {
        var filterValue = target.value;
        var filterCheck = target.checked;
        this.updateFilterByValues(name, filterValue, filterCheck ? ItemOption.Include : ItemOption.None);
    },
    updateFilterByValues: function (name, value, option) {
        var filter = this.filters.find(function (item) { return item.key === name; });
        if (!filter) {
            return;
        }
        var filterItem = filter.filterItems.find(function (item) { return item.value === value; });
        if (!filterItem) {
            return;
        }
        if (filterItem.option === option) {
            return;
        }
        filterItem.option = option;
        if (filterItem.element) {
            if (filterItem.option === ItemOption.Include) {
                filterItem.element.checked = true;
            }
            else if (filterItem.option === ItemOption.None) {
                filterItem.element.checked = false;
            }
        }
        this.resetPageNumber();
        this.updateUrl();
        this.updateOverview();
    },
    addFilterValue: function (name, value, label) {
        var filter = this.filters.find(function (filter) { return filter.key === name; });
        if (!filter) {
            filter = {
                key: name,
                filterItems: [],
                elementLess: true
            };
            this.filters.push(filter);
        }
        filter.filterItems.push({
            value: value,
            label: label,
            option: ItemOption.Include
        });
        this.resetPageNumber();
        this.updateUrl();
        this.updateOverview();
    },
    onUpdateSearch: function () {
        this.resetPageNumber();
        this.updateUrl();
    },
    setPageNumber: function (page) {
        this.pageNumber = page;
        this.updateUrl();
        this.updateOverview();
    },
    resetPageNumber: function () {
        this.pageNumber = undefined;
    },
    updateOverview: function () {
        var _this = this;
        var target = this.$refs.submitForm;
        var data = new FormData(target);
        data.append("isAjax", "true");
        if (this.pageNumber) {
            data.append("pageNumber", this.pageNumber);
        }
        var elementLessFilters = this.filters.filter(function (item) { return item.elementLess; });
        elementLessFilters.forEach(function (filter) {
            var items = filter.filterItems.filter(function (item) { return item.option === ItemOption.Include; });
            if (items.length === 0)
                return;
            data.append("Filters[".concat(filter.key, "]"), items.map(function (item) { return item.value; }).join(","));
        });
        this.loading = true;
        fetch(this.$refs.submitForm.action, {
            method: "POST",
            body: data
        })
            .then(function (res) { return res.text(); })
            .then(function (res) {
            var cardOverviewElem = document.getElementById("card-overview");
            cardOverviewElem.innerHTML = res;
            // @ts-ignore
            htmx.process(cardOverviewElem);
            _this.loading = false;
            window.dispatchEvent(new CustomEvent('deck-overview-updated'));
        });
    },
    resetFilters: function () {
        var _this = this;
        this.filters.forEach(function (filter) {
            filter.filterItems.forEach(function (item) {
                if (item.option === ItemOption.None) {
                    return false;
                }
                _this.removeFilter(item);
            });
        });
    },
    removeFilter: function (filterItem) {
        filterItem.option = ItemOption.None;
        if (filterItem.element) {
            filterItem.element.checked = false;
        }
        this.updateUrl();
        this.updateOverview();
    },
    updateUrl: function () {
        if (!config.changeUrl) {
            return;
        }
        var url = new URL(window.location.href.split("?")[0]);
        this.filters.forEach(function (filter) {
            var checkedItems = filter.filterItems.filter(function (item) { return item.option === ItemOption.Include; });
            if (checkedItems.length === 0) {
                url.searchParams["delete"](filter.key);
                return;
            }
            checkedItems.forEach(function (item) {
                url.searchParams.append(filter.key, item.value);
            });
        });
        if (this.pageNumber) {
            url.searchParams.append("page", this.pageNumber);
        }
        history.replaceState({}, null, url);
    }
}); });
var ItemOption;
(function (ItemOption) {
    ItemOption[ItemOption["None"] = 0] = "None";
    ItemOption[ItemOption["Include"] = 1] = "Include";
    ItemOption[ItemOption["Exclude"] = 2] = "Exclude";
})(ItemOption || (ItemOption = {}));


/***/ }),

/***/ "./js/tooltip.ts":
/*!***********************!*\
  !*** ./js/tooltip.ts ***!
  \***********************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () {
    document.querySelectorAll(".tooltip-starter").forEach(function (element) {
        var tooltips = element.querySelectorAll(".tooltip");
        var parentBounding = element.getBoundingClientRect();
        tooltips.forEach(function (item) {
            var bottomXTooltip = 300 + parentBounding.y;
            if (bottomXTooltip >= window.innerHeight) {
                item.classList.add("top");
            }
        });
        element.addEventListener("mouseenter", function () {
            tooltips.forEach(function (item) {
                item.classList.add("shown");
            });
        });
        element.addEventListener("mouseleave", function () {
            tooltips.forEach(function (item) { return item.classList.remove("shown"); });
        });
    });
    document.querySelectorAll(".tooltip-container").forEach(function (element) {
        element.addEventListener("mouseenter", function (event) {
            console.log(event);
        }, true);
    });
});


/***/ }),

/***/ "./js/utils/toggler.ts":
/*!*****************************!*\
  !*** ./js/utils/toggler.ts ***!
  \*****************************/
/***/ ((__unused_webpack_module, exports) => {


exports.__esModule = true;
exports["default"] = (function () {
    var togglers = Array.from(document.querySelectorAll("[data-toggle]"));
    togglers.forEach(function (element) {
        var toggleElements = element.dataset.toggle;
        element.addEventListener("input", function () {
            var toggleElements = document.querySelectorAll(element.dataset.toggle);
            toggleElements.forEach(function (el) { return el.style.display = el.style.display == 'none' ? '' : 'none'; });
        });
    });
});


/***/ }),

/***/ "./node_modules/@floating-ui/core/dist/floating-ui.core.mjs":
/*!******************************************************************!*\
  !*** ./node_modules/@floating-ui/core/dist/floating-ui.core.mjs ***!
  \******************************************************************/
/***/ ((__unused_webpack___webpack_module__, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "arrow": () => (/* binding */ arrow),
/* harmony export */   "autoPlacement": () => (/* binding */ autoPlacement),
/* harmony export */   "computePosition": () => (/* binding */ computePosition),
/* harmony export */   "detectOverflow": () => (/* binding */ detectOverflow),
/* harmony export */   "flip": () => (/* binding */ flip),
/* harmony export */   "hide": () => (/* binding */ hide),
/* harmony export */   "inline": () => (/* binding */ inline),
/* harmony export */   "limitShift": () => (/* binding */ limitShift),
/* harmony export */   "offset": () => (/* binding */ offset),
/* harmony export */   "rectToClientRect": () => (/* reexport safe */ _floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.rectToClientRect),
/* harmony export */   "shift": () => (/* binding */ shift),
/* harmony export */   "size": () => (/* binding */ size)
/* harmony export */ });
/* harmony import */ var _floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @floating-ui/utils */ "./node_modules/@floating-ui/utils/dist/floating-ui.utils.mjs");



function computeCoordsFromPlacement(_ref, placement, rtl) {
  let {
    reference,
    floating
  } = _ref;
  const sideAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSideAxis)(placement);
  const alignmentAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignmentAxis)(placement);
  const alignLength = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAxisLength)(alignmentAxis);
  const side = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement);
  const isVertical = sideAxis === 'y';
  const commonX = reference.x + reference.width / 2 - floating.width / 2;
  const commonY = reference.y + reference.height / 2 - floating.height / 2;
  const commonAlign = reference[alignLength] / 2 - floating[alignLength] / 2;
  let coords;
  switch (side) {
    case 'top':
      coords = {
        x: commonX,
        y: reference.y - floating.height
      };
      break;
    case 'bottom':
      coords = {
        x: commonX,
        y: reference.y + reference.height
      };
      break;
    case 'right':
      coords = {
        x: reference.x + reference.width,
        y: commonY
      };
      break;
    case 'left':
      coords = {
        x: reference.x - floating.width,
        y: commonY
      };
      break;
    default:
      coords = {
        x: reference.x,
        y: reference.y
      };
  }
  switch ((0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(placement)) {
    case 'start':
      coords[alignmentAxis] -= commonAlign * (rtl && isVertical ? -1 : 1);
      break;
    case 'end':
      coords[alignmentAxis] += commonAlign * (rtl && isVertical ? -1 : 1);
      break;
  }
  return coords;
}

/**
 * Computes the `x` and `y` coordinates that will place the floating element
 * next to a given reference element.
 *
 * This export does not have any `platform` interface logic. You will need to
 * write one for the platform you are using Floating UI with.
 */
const computePosition = async (reference, floating, config) => {
  const {
    placement = 'bottom',
    strategy = 'absolute',
    middleware = [],
    platform
  } = config;
  const validMiddleware = middleware.filter(Boolean);
  const rtl = await (platform.isRTL == null ? void 0 : platform.isRTL(floating));
  let rects = await platform.getElementRects({
    reference,
    floating,
    strategy
  });
  let {
    x,
    y
  } = computeCoordsFromPlacement(rects, placement, rtl);
  let statefulPlacement = placement;
  let middlewareData = {};
  let resetCount = 0;
  for (let i = 0; i < validMiddleware.length; i++) {
    const {
      name,
      fn
    } = validMiddleware[i];
    const {
      x: nextX,
      y: nextY,
      data,
      reset
    } = await fn({
      x,
      y,
      initialPlacement: placement,
      placement: statefulPlacement,
      strategy,
      middlewareData,
      rects,
      platform,
      elements: {
        reference,
        floating
      }
    });
    x = nextX != null ? nextX : x;
    y = nextY != null ? nextY : y;
    middlewareData = {
      ...middlewareData,
      [name]: {
        ...middlewareData[name],
        ...data
      }
    };
    if (reset && resetCount <= 50) {
      resetCount++;
      if (typeof reset === 'object') {
        if (reset.placement) {
          statefulPlacement = reset.placement;
        }
        if (reset.rects) {
          rects = reset.rects === true ? await platform.getElementRects({
            reference,
            floating,
            strategy
          }) : reset.rects;
        }
        ({
          x,
          y
        } = computeCoordsFromPlacement(rects, statefulPlacement, rtl));
      }
      i = -1;
    }
  }
  return {
    x,
    y,
    placement: statefulPlacement,
    strategy,
    middlewareData
  };
};

/**
 * Resolves with an object of overflow side offsets that determine how much the
 * element is overflowing a given clipping boundary on each side.
 * - positive = overflowing the boundary by that number of pixels
 * - negative = how many pixels left before it will overflow
 * - 0 = lies flush with the boundary
 * @see https://floating-ui.com/docs/detectOverflow
 */
async function detectOverflow(state, options) {
  var _await$platform$isEle;
  if (options === void 0) {
    options = {};
  }
  const {
    x,
    y,
    platform,
    rects,
    elements,
    strategy
  } = state;
  const {
    boundary = 'clippingAncestors',
    rootBoundary = 'viewport',
    elementContext = 'floating',
    altBoundary = false,
    padding = 0
  } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);
  const paddingObject = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getPaddingObject)(padding);
  const altContext = elementContext === 'floating' ? 'reference' : 'floating';
  const element = elements[altBoundary ? altContext : elementContext];
  const clippingClientRect = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.rectToClientRect)(await platform.getClippingRect({
    element: ((_await$platform$isEle = await (platform.isElement == null ? void 0 : platform.isElement(element))) != null ? _await$platform$isEle : true) ? element : element.contextElement || (await (platform.getDocumentElement == null ? void 0 : platform.getDocumentElement(elements.floating))),
    boundary,
    rootBoundary,
    strategy
  }));
  const rect = elementContext === 'floating' ? {
    x,
    y,
    width: rects.floating.width,
    height: rects.floating.height
  } : rects.reference;
  const offsetParent = await (platform.getOffsetParent == null ? void 0 : platform.getOffsetParent(elements.floating));
  const offsetScale = (await (platform.isElement == null ? void 0 : platform.isElement(offsetParent))) ? (await (platform.getScale == null ? void 0 : platform.getScale(offsetParent))) || {
    x: 1,
    y: 1
  } : {
    x: 1,
    y: 1
  };
  const elementClientRect = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.rectToClientRect)(platform.convertOffsetParentRelativeRectToViewportRelativeRect ? await platform.convertOffsetParentRelativeRectToViewportRelativeRect({
    elements,
    rect,
    offsetParent,
    strategy
  }) : rect);
  return {
    top: (clippingClientRect.top - elementClientRect.top + paddingObject.top) / offsetScale.y,
    bottom: (elementClientRect.bottom - clippingClientRect.bottom + paddingObject.bottom) / offsetScale.y,
    left: (clippingClientRect.left - elementClientRect.left + paddingObject.left) / offsetScale.x,
    right: (elementClientRect.right - clippingClientRect.right + paddingObject.right) / offsetScale.x
  };
}

/**
 * Provides data to position an inner element of the floating element so that it
 * appears centered to the reference element.
 * @see https://floating-ui.com/docs/arrow
 */
const arrow = options => ({
  name: 'arrow',
  options,
  async fn(state) {
    const {
      x,
      y,
      placement,
      rects,
      platform,
      elements,
      middlewareData
    } = state;
    // Since `element` is required, we don't Partial<> the type.
    const {
      element,
      padding = 0
    } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state) || {};
    if (element == null) {
      return {};
    }
    const paddingObject = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getPaddingObject)(padding);
    const coords = {
      x,
      y
    };
    const axis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignmentAxis)(placement);
    const length = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAxisLength)(axis);
    const arrowDimensions = await platform.getDimensions(element);
    const isYAxis = axis === 'y';
    const minProp = isYAxis ? 'top' : 'left';
    const maxProp = isYAxis ? 'bottom' : 'right';
    const clientProp = isYAxis ? 'clientHeight' : 'clientWidth';
    const endDiff = rects.reference[length] + rects.reference[axis] - coords[axis] - rects.floating[length];
    const startDiff = coords[axis] - rects.reference[axis];
    const arrowOffsetParent = await (platform.getOffsetParent == null ? void 0 : platform.getOffsetParent(element));
    let clientSize = arrowOffsetParent ? arrowOffsetParent[clientProp] : 0;

    // DOM platform can return `window` as the `offsetParent`.
    if (!clientSize || !(await (platform.isElement == null ? void 0 : platform.isElement(arrowOffsetParent)))) {
      clientSize = elements.floating[clientProp] || rects.floating[length];
    }
    const centerToReference = endDiff / 2 - startDiff / 2;

    // If the padding is large enough that it causes the arrow to no longer be
    // centered, modify the padding so that it is centered.
    const largestPossiblePadding = clientSize / 2 - arrowDimensions[length] / 2 - 1;
    const minPadding = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.min)(paddingObject[minProp], largestPossiblePadding);
    const maxPadding = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.min)(paddingObject[maxProp], largestPossiblePadding);

    // Make sure the arrow doesn't overflow the floating element if the center
    // point is outside the floating element's bounds.
    const min$1 = minPadding;
    const max = clientSize - arrowDimensions[length] - maxPadding;
    const center = clientSize / 2 - arrowDimensions[length] / 2 + centerToReference;
    const offset = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.clamp)(min$1, center, max);

    // If the reference is small enough that the arrow's padding causes it to
    // to point to nothing for an aligned placement, adjust the offset of the
    // floating element itself. To ensure `shift()` continues to take action,
    // a single reset is performed when this is true.
    const shouldAddOffset = !middlewareData.arrow && (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(placement) != null && center !== offset && rects.reference[length] / 2 - (center < min$1 ? minPadding : maxPadding) - arrowDimensions[length] / 2 < 0;
    const alignmentOffset = shouldAddOffset ? center < min$1 ? center - min$1 : center - max : 0;
    return {
      [axis]: coords[axis] + alignmentOffset,
      data: {
        [axis]: offset,
        centerOffset: center - offset - alignmentOffset,
        ...(shouldAddOffset && {
          alignmentOffset
        })
      },
      reset: shouldAddOffset
    };
  }
});

function getPlacementList(alignment, autoAlignment, allowedPlacements) {
  const allowedPlacementsSortedByAlignment = alignment ? [...allowedPlacements.filter(placement => (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(placement) === alignment), ...allowedPlacements.filter(placement => (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(placement) !== alignment)] : allowedPlacements.filter(placement => (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement) === placement);
  return allowedPlacementsSortedByAlignment.filter(placement => {
    if (alignment) {
      return (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(placement) === alignment || (autoAlignment ? (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getOppositeAlignmentPlacement)(placement) !== placement : false);
    }
    return true;
  });
}
/**
 * Optimizes the visibility of the floating element by choosing the placement
 * that has the most space available automatically, without needing to specify a
 * preferred placement. Alternative to `flip`.
 * @see https://floating-ui.com/docs/autoPlacement
 */
const autoPlacement = function (options) {
  if (options === void 0) {
    options = {};
  }
  return {
    name: 'autoPlacement',
    options,
    async fn(state) {
      var _middlewareData$autoP, _middlewareData$autoP2, _placementsThatFitOnE;
      const {
        rects,
        middlewareData,
        placement,
        platform,
        elements
      } = state;
      const {
        crossAxis = false,
        alignment,
        allowedPlacements = _floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.placements,
        autoAlignment = true,
        ...detectOverflowOptions
      } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);
      const placements$1 = alignment !== undefined || allowedPlacements === _floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.placements ? getPlacementList(alignment || null, autoAlignment, allowedPlacements) : allowedPlacements;
      const overflow = await detectOverflow(state, detectOverflowOptions);
      const currentIndex = ((_middlewareData$autoP = middlewareData.autoPlacement) == null ? void 0 : _middlewareData$autoP.index) || 0;
      const currentPlacement = placements$1[currentIndex];
      if (currentPlacement == null) {
        return {};
      }
      const alignmentSides = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignmentSides)(currentPlacement, rects, await (platform.isRTL == null ? void 0 : platform.isRTL(elements.floating)));

      // Make `computeCoords` start from the right place.
      if (placement !== currentPlacement) {
        return {
          reset: {
            placement: placements$1[0]
          }
        };
      }
      const currentOverflows = [overflow[(0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(currentPlacement)], overflow[alignmentSides[0]], overflow[alignmentSides[1]]];
      const allOverflows = [...(((_middlewareData$autoP2 = middlewareData.autoPlacement) == null ? void 0 : _middlewareData$autoP2.overflows) || []), {
        placement: currentPlacement,
        overflows: currentOverflows
      }];
      const nextPlacement = placements$1[currentIndex + 1];

      // There are more placements to check.
      if (nextPlacement) {
        return {
          data: {
            index: currentIndex + 1,
            overflows: allOverflows
          },
          reset: {
            placement: nextPlacement
          }
        };
      }
      const placementsSortedByMostSpace = allOverflows.map(d => {
        const alignment = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(d.placement);
        return [d.placement, alignment && crossAxis ?
        // Check along the mainAxis and main crossAxis side.
        d.overflows.slice(0, 2).reduce((acc, v) => acc + v, 0) :
        // Check only the mainAxis.
        d.overflows[0], d.overflows];
      }).sort((a, b) => a[1] - b[1]);
      const placementsThatFitOnEachSide = placementsSortedByMostSpace.filter(d => d[2].slice(0,
      // Aligned placements should not check their opposite crossAxis
      // side.
      (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(d[0]) ? 2 : 3).every(v => v <= 0));
      const resetPlacement = ((_placementsThatFitOnE = placementsThatFitOnEachSide[0]) == null ? void 0 : _placementsThatFitOnE[0]) || placementsSortedByMostSpace[0][0];
      if (resetPlacement !== placement) {
        return {
          data: {
            index: currentIndex + 1,
            overflows: allOverflows
          },
          reset: {
            placement: resetPlacement
          }
        };
      }
      return {};
    }
  };
};

/**
 * Optimizes the visibility of the floating element by flipping the `placement`
 * in order to keep it in view when the preferred placement(s) will overflow the
 * clipping boundary. Alternative to `autoPlacement`.
 * @see https://floating-ui.com/docs/flip
 */
const flip = function (options) {
  if (options === void 0) {
    options = {};
  }
  return {
    name: 'flip',
    options,
    async fn(state) {
      var _middlewareData$arrow, _middlewareData$flip;
      const {
        placement,
        middlewareData,
        rects,
        initialPlacement,
        platform,
        elements
      } = state;
      const {
        mainAxis: checkMainAxis = true,
        crossAxis: checkCrossAxis = true,
        fallbackPlacements: specifiedFallbackPlacements,
        fallbackStrategy = 'bestFit',
        fallbackAxisSideDirection = 'none',
        flipAlignment = true,
        ...detectOverflowOptions
      } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);

      // If a reset by the arrow was caused due to an alignment offset being
      // added, we should skip any logic now since `flip()` has already done its
      // work.
      // https://github.com/floating-ui/floating-ui/issues/2549#issuecomment-1719601643
      if ((_middlewareData$arrow = middlewareData.arrow) != null && _middlewareData$arrow.alignmentOffset) {
        return {};
      }
      const side = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement);
      const initialSideAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSideAxis)(initialPlacement);
      const isBasePlacement = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(initialPlacement) === initialPlacement;
      const rtl = await (platform.isRTL == null ? void 0 : platform.isRTL(elements.floating));
      const fallbackPlacements = specifiedFallbackPlacements || (isBasePlacement || !flipAlignment ? [(0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getOppositePlacement)(initialPlacement)] : (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getExpandedPlacements)(initialPlacement));
      const hasFallbackAxisSideDirection = fallbackAxisSideDirection !== 'none';
      if (!specifiedFallbackPlacements && hasFallbackAxisSideDirection) {
        fallbackPlacements.push(...(0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getOppositeAxisPlacements)(initialPlacement, flipAlignment, fallbackAxisSideDirection, rtl));
      }
      const placements = [initialPlacement, ...fallbackPlacements];
      const overflow = await detectOverflow(state, detectOverflowOptions);
      const overflows = [];
      let overflowsData = ((_middlewareData$flip = middlewareData.flip) == null ? void 0 : _middlewareData$flip.overflows) || [];
      if (checkMainAxis) {
        overflows.push(overflow[side]);
      }
      if (checkCrossAxis) {
        const sides = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignmentSides)(placement, rects, rtl);
        overflows.push(overflow[sides[0]], overflow[sides[1]]);
      }
      overflowsData = [...overflowsData, {
        placement,
        overflows
      }];

      // One or more sides is overflowing.
      if (!overflows.every(side => side <= 0)) {
        var _middlewareData$flip2, _overflowsData$filter;
        const nextIndex = (((_middlewareData$flip2 = middlewareData.flip) == null ? void 0 : _middlewareData$flip2.index) || 0) + 1;
        const nextPlacement = placements[nextIndex];
        if (nextPlacement) {
          // Try next placement and re-run the lifecycle.
          return {
            data: {
              index: nextIndex,
              overflows: overflowsData
            },
            reset: {
              placement: nextPlacement
            }
          };
        }

        // First, find the candidates that fit on the mainAxis side of overflow,
        // then find the placement that fits the best on the main crossAxis side.
        let resetPlacement = (_overflowsData$filter = overflowsData.filter(d => d.overflows[0] <= 0).sort((a, b) => a.overflows[1] - b.overflows[1])[0]) == null ? void 0 : _overflowsData$filter.placement;

        // Otherwise fallback.
        if (!resetPlacement) {
          switch (fallbackStrategy) {
            case 'bestFit':
              {
                var _overflowsData$filter2;
                const placement = (_overflowsData$filter2 = overflowsData.filter(d => {
                  if (hasFallbackAxisSideDirection) {
                    const currentSideAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSideAxis)(d.placement);
                    return currentSideAxis === initialSideAxis ||
                    // Create a bias to the `y` side axis due to horizontal
                    // reading directions favoring greater width.
                    currentSideAxis === 'y';
                  }
                  return true;
                }).map(d => [d.placement, d.overflows.filter(overflow => overflow > 0).reduce((acc, overflow) => acc + overflow, 0)]).sort((a, b) => a[1] - b[1])[0]) == null ? void 0 : _overflowsData$filter2[0];
                if (placement) {
                  resetPlacement = placement;
                }
                break;
              }
            case 'initialPlacement':
              resetPlacement = initialPlacement;
              break;
          }
        }
        if (placement !== resetPlacement) {
          return {
            reset: {
              placement: resetPlacement
            }
          };
        }
      }
      return {};
    }
  };
};

function getSideOffsets(overflow, rect) {
  return {
    top: overflow.top - rect.height,
    right: overflow.right - rect.width,
    bottom: overflow.bottom - rect.height,
    left: overflow.left - rect.width
  };
}
function isAnySideFullyClipped(overflow) {
  return _floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.sides.some(side => overflow[side] >= 0);
}
/**
 * Provides data to hide the floating element in applicable situations, such as
 * when it is not in the same clipping context as the reference element.
 * @see https://floating-ui.com/docs/hide
 */
const hide = function (options) {
  if (options === void 0) {
    options = {};
  }
  return {
    name: 'hide',
    options,
    async fn(state) {
      const {
        rects
      } = state;
      const {
        strategy = 'referenceHidden',
        ...detectOverflowOptions
      } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);
      switch (strategy) {
        case 'referenceHidden':
          {
            const overflow = await detectOverflow(state, {
              ...detectOverflowOptions,
              elementContext: 'reference'
            });
            const offsets = getSideOffsets(overflow, rects.reference);
            return {
              data: {
                referenceHiddenOffsets: offsets,
                referenceHidden: isAnySideFullyClipped(offsets)
              }
            };
          }
        case 'escaped':
          {
            const overflow = await detectOverflow(state, {
              ...detectOverflowOptions,
              altBoundary: true
            });
            const offsets = getSideOffsets(overflow, rects.floating);
            return {
              data: {
                escapedOffsets: offsets,
                escaped: isAnySideFullyClipped(offsets)
              }
            };
          }
        default:
          {
            return {};
          }
      }
    }
  };
};

function getBoundingRect(rects) {
  const minX = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.min)(...rects.map(rect => rect.left));
  const minY = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.min)(...rects.map(rect => rect.top));
  const maxX = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(...rects.map(rect => rect.right));
  const maxY = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(...rects.map(rect => rect.bottom));
  return {
    x: minX,
    y: minY,
    width: maxX - minX,
    height: maxY - minY
  };
}
function getRectsByLine(rects) {
  const sortedRects = rects.slice().sort((a, b) => a.y - b.y);
  const groups = [];
  let prevRect = null;
  for (let i = 0; i < sortedRects.length; i++) {
    const rect = sortedRects[i];
    if (!prevRect || rect.y - prevRect.y > prevRect.height / 2) {
      groups.push([rect]);
    } else {
      groups[groups.length - 1].push(rect);
    }
    prevRect = rect;
  }
  return groups.map(rect => (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.rectToClientRect)(getBoundingRect(rect)));
}
/**
 * Provides improved positioning for inline reference elements that can span
 * over multiple lines, such as hyperlinks or range selections.
 * @see https://floating-ui.com/docs/inline
 */
const inline = function (options) {
  if (options === void 0) {
    options = {};
  }
  return {
    name: 'inline',
    options,
    async fn(state) {
      const {
        placement,
        elements,
        rects,
        platform,
        strategy
      } = state;
      // A MouseEvent's client{X,Y} coords can be up to 2 pixels off a
      // ClientRect's bounds, despite the event listener being triggered. A
      // padding of 2 seems to handle this issue.
      const {
        padding = 2,
        x,
        y
      } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);
      const nativeClientRects = Array.from((await (platform.getClientRects == null ? void 0 : platform.getClientRects(elements.reference))) || []);
      const clientRects = getRectsByLine(nativeClientRects);
      const fallback = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.rectToClientRect)(getBoundingRect(nativeClientRects));
      const paddingObject = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getPaddingObject)(padding);
      function getBoundingClientRect() {
        // There are two rects and they are disjoined.
        if (clientRects.length === 2 && clientRects[0].left > clientRects[1].right && x != null && y != null) {
          // Find the first rect in which the point is fully inside.
          return clientRects.find(rect => x > rect.left - paddingObject.left && x < rect.right + paddingObject.right && y > rect.top - paddingObject.top && y < rect.bottom + paddingObject.bottom) || fallback;
        }

        // There are 2 or more connected rects.
        if (clientRects.length >= 2) {
          if ((0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSideAxis)(placement) === 'y') {
            const firstRect = clientRects[0];
            const lastRect = clientRects[clientRects.length - 1];
            const isTop = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement) === 'top';
            const top = firstRect.top;
            const bottom = lastRect.bottom;
            const left = isTop ? firstRect.left : lastRect.left;
            const right = isTop ? firstRect.right : lastRect.right;
            const width = right - left;
            const height = bottom - top;
            return {
              top,
              bottom,
              left,
              right,
              width,
              height,
              x: left,
              y: top
            };
          }
          const isLeftSide = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement) === 'left';
          const maxRight = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(...clientRects.map(rect => rect.right));
          const minLeft = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.min)(...clientRects.map(rect => rect.left));
          const measureRects = clientRects.filter(rect => isLeftSide ? rect.left === minLeft : rect.right === maxRight);
          const top = measureRects[0].top;
          const bottom = measureRects[measureRects.length - 1].bottom;
          const left = minLeft;
          const right = maxRight;
          const width = right - left;
          const height = bottom - top;
          return {
            top,
            bottom,
            left,
            right,
            width,
            height,
            x: left,
            y: top
          };
        }
        return fallback;
      }
      const resetRects = await platform.getElementRects({
        reference: {
          getBoundingClientRect
        },
        floating: elements.floating,
        strategy
      });
      if (rects.reference.x !== resetRects.reference.x || rects.reference.y !== resetRects.reference.y || rects.reference.width !== resetRects.reference.width || rects.reference.height !== resetRects.reference.height) {
        return {
          reset: {
            rects: resetRects
          }
        };
      }
      return {};
    }
  };
};

// For type backwards-compatibility, the `OffsetOptions` type was also
// Derivable.

async function convertValueToCoords(state, options) {
  const {
    placement,
    platform,
    elements
  } = state;
  const rtl = await (platform.isRTL == null ? void 0 : platform.isRTL(elements.floating));
  const side = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement);
  const alignment = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(placement);
  const isVertical = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSideAxis)(placement) === 'y';
  const mainAxisMulti = ['left', 'top'].includes(side) ? -1 : 1;
  const crossAxisMulti = rtl && isVertical ? -1 : 1;
  const rawValue = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);

  // eslint-disable-next-line prefer-const
  let {
    mainAxis,
    crossAxis,
    alignmentAxis
  } = typeof rawValue === 'number' ? {
    mainAxis: rawValue,
    crossAxis: 0,
    alignmentAxis: null
  } : {
    mainAxis: rawValue.mainAxis || 0,
    crossAxis: rawValue.crossAxis || 0,
    alignmentAxis: rawValue.alignmentAxis
  };
  if (alignment && typeof alignmentAxis === 'number') {
    crossAxis = alignment === 'end' ? alignmentAxis * -1 : alignmentAxis;
  }
  return isVertical ? {
    x: crossAxis * crossAxisMulti,
    y: mainAxis * mainAxisMulti
  } : {
    x: mainAxis * mainAxisMulti,
    y: crossAxis * crossAxisMulti
  };
}

/**
 * Modifies the placement by translating the floating element along the
 * specified axes.
 * A number (shorthand for `mainAxis` or distance), or an axes configuration
 * object may be passed.
 * @see https://floating-ui.com/docs/offset
 */
const offset = function (options) {
  if (options === void 0) {
    options = 0;
  }
  return {
    name: 'offset',
    options,
    async fn(state) {
      var _middlewareData$offse, _middlewareData$arrow;
      const {
        x,
        y,
        placement,
        middlewareData
      } = state;
      const diffCoords = await convertValueToCoords(state, options);

      // If the placement is the same and the arrow caused an alignment offset
      // then we don't need to change the positioning coordinates.
      if (placement === ((_middlewareData$offse = middlewareData.offset) == null ? void 0 : _middlewareData$offse.placement) && (_middlewareData$arrow = middlewareData.arrow) != null && _middlewareData$arrow.alignmentOffset) {
        return {};
      }
      return {
        x: x + diffCoords.x,
        y: y + diffCoords.y,
        data: {
          ...diffCoords,
          placement
        }
      };
    }
  };
};

/**
 * Optimizes the visibility of the floating element by shifting it in order to
 * keep it in view when it will overflow the clipping boundary.
 * @see https://floating-ui.com/docs/shift
 */
const shift = function (options) {
  if (options === void 0) {
    options = {};
  }
  return {
    name: 'shift',
    options,
    async fn(state) {
      const {
        x,
        y,
        placement
      } = state;
      const {
        mainAxis: checkMainAxis = true,
        crossAxis: checkCrossAxis = false,
        limiter = {
          fn: _ref => {
            let {
              x,
              y
            } = _ref;
            return {
              x,
              y
            };
          }
        },
        ...detectOverflowOptions
      } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);
      const coords = {
        x,
        y
      };
      const overflow = await detectOverflow(state, detectOverflowOptions);
      const crossAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSideAxis)((0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement));
      const mainAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getOppositeAxis)(crossAxis);
      let mainAxisCoord = coords[mainAxis];
      let crossAxisCoord = coords[crossAxis];
      if (checkMainAxis) {
        const minSide = mainAxis === 'y' ? 'top' : 'left';
        const maxSide = mainAxis === 'y' ? 'bottom' : 'right';
        const min = mainAxisCoord + overflow[minSide];
        const max = mainAxisCoord - overflow[maxSide];
        mainAxisCoord = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.clamp)(min, mainAxisCoord, max);
      }
      if (checkCrossAxis) {
        const minSide = crossAxis === 'y' ? 'top' : 'left';
        const maxSide = crossAxis === 'y' ? 'bottom' : 'right';
        const min = crossAxisCoord + overflow[minSide];
        const max = crossAxisCoord - overflow[maxSide];
        crossAxisCoord = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.clamp)(min, crossAxisCoord, max);
      }
      const limitedCoords = limiter.fn({
        ...state,
        [mainAxis]: mainAxisCoord,
        [crossAxis]: crossAxisCoord
      });
      return {
        ...limitedCoords,
        data: {
          x: limitedCoords.x - x,
          y: limitedCoords.y - y,
          enabled: {
            [mainAxis]: checkMainAxis,
            [crossAxis]: checkCrossAxis
          }
        }
      };
    }
  };
};
/**
 * Built-in `limiter` that will stop `shift()` at a certain point.
 */
const limitShift = function (options) {
  if (options === void 0) {
    options = {};
  }
  return {
    options,
    fn(state) {
      const {
        x,
        y,
        placement,
        rects,
        middlewareData
      } = state;
      const {
        offset = 0,
        mainAxis: checkMainAxis = true,
        crossAxis: checkCrossAxis = true
      } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);
      const coords = {
        x,
        y
      };
      const crossAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSideAxis)(placement);
      const mainAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getOppositeAxis)(crossAxis);
      let mainAxisCoord = coords[mainAxis];
      let crossAxisCoord = coords[crossAxis];
      const rawOffset = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(offset, state);
      const computedOffset = typeof rawOffset === 'number' ? {
        mainAxis: rawOffset,
        crossAxis: 0
      } : {
        mainAxis: 0,
        crossAxis: 0,
        ...rawOffset
      };
      if (checkMainAxis) {
        const len = mainAxis === 'y' ? 'height' : 'width';
        const limitMin = rects.reference[mainAxis] - rects.floating[len] + computedOffset.mainAxis;
        const limitMax = rects.reference[mainAxis] + rects.reference[len] - computedOffset.mainAxis;
        if (mainAxisCoord < limitMin) {
          mainAxisCoord = limitMin;
        } else if (mainAxisCoord > limitMax) {
          mainAxisCoord = limitMax;
        }
      }
      if (checkCrossAxis) {
        var _middlewareData$offse, _middlewareData$offse2;
        const len = mainAxis === 'y' ? 'width' : 'height';
        const isOriginSide = ['top', 'left'].includes((0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement));
        const limitMin = rects.reference[crossAxis] - rects.floating[len] + (isOriginSide ? ((_middlewareData$offse = middlewareData.offset) == null ? void 0 : _middlewareData$offse[crossAxis]) || 0 : 0) + (isOriginSide ? 0 : computedOffset.crossAxis);
        const limitMax = rects.reference[crossAxis] + rects.reference[len] + (isOriginSide ? 0 : ((_middlewareData$offse2 = middlewareData.offset) == null ? void 0 : _middlewareData$offse2[crossAxis]) || 0) - (isOriginSide ? computedOffset.crossAxis : 0);
        if (crossAxisCoord < limitMin) {
          crossAxisCoord = limitMin;
        } else if (crossAxisCoord > limitMax) {
          crossAxisCoord = limitMax;
        }
      }
      return {
        [mainAxis]: mainAxisCoord,
        [crossAxis]: crossAxisCoord
      };
    }
  };
};

/**
 * Provides data that allows you to change the size of the floating element —
 * for instance, prevent it from overflowing the clipping boundary or match the
 * width of the reference element.
 * @see https://floating-ui.com/docs/size
 */
const size = function (options) {
  if (options === void 0) {
    options = {};
  }
  return {
    name: 'size',
    options,
    async fn(state) {
      var _state$middlewareData, _state$middlewareData2;
      const {
        placement,
        rects,
        platform,
        elements
      } = state;
      const {
        apply = () => {},
        ...detectOverflowOptions
      } = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.evaluate)(options, state);
      const overflow = await detectOverflow(state, detectOverflowOptions);
      const side = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSide)(placement);
      const alignment = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getAlignment)(placement);
      const isYAxis = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.getSideAxis)(placement) === 'y';
      const {
        width,
        height
      } = rects.floating;
      let heightSide;
      let widthSide;
      if (side === 'top' || side === 'bottom') {
        heightSide = side;
        widthSide = alignment === ((await (platform.isRTL == null ? void 0 : platform.isRTL(elements.floating))) ? 'start' : 'end') ? 'left' : 'right';
      } else {
        widthSide = side;
        heightSide = alignment === 'end' ? 'top' : 'bottom';
      }
      const maximumClippingHeight = height - overflow.top - overflow.bottom;
      const maximumClippingWidth = width - overflow.left - overflow.right;
      const overflowAvailableHeight = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.min)(height - overflow[heightSide], maximumClippingHeight);
      const overflowAvailableWidth = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.min)(width - overflow[widthSide], maximumClippingWidth);
      const noShift = !state.middlewareData.shift;
      let availableHeight = overflowAvailableHeight;
      let availableWidth = overflowAvailableWidth;
      if ((_state$middlewareData = state.middlewareData.shift) != null && _state$middlewareData.enabled.x) {
        availableWidth = maximumClippingWidth;
      }
      if ((_state$middlewareData2 = state.middlewareData.shift) != null && _state$middlewareData2.enabled.y) {
        availableHeight = maximumClippingHeight;
      }
      if (noShift && !alignment) {
        const xMin = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(overflow.left, 0);
        const xMax = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(overflow.right, 0);
        const yMin = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(overflow.top, 0);
        const yMax = (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(overflow.bottom, 0);
        if (isYAxis) {
          availableWidth = width - 2 * (xMin !== 0 || xMax !== 0 ? xMin + xMax : (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(overflow.left, overflow.right));
        } else {
          availableHeight = height - 2 * (yMin !== 0 || yMax !== 0 ? yMin + yMax : (0,_floating_ui_utils__WEBPACK_IMPORTED_MODULE_0__.max)(overflow.top, overflow.bottom));
        }
      }
      await apply({
        ...state,
        availableWidth,
        availableHeight
      });
      const nextDimensions = await platform.getDimensions(elements.floating);
      if (width !== nextDimensions.width || height !== nextDimensions.height) {
        return {
          reset: {
            rects: true
          }
        };
      }
      return {};
    }
  };
};




/***/ }),

/***/ "./node_modules/@floating-ui/utils/dist/floating-ui.utils.dom.mjs":
/*!************************************************************************!*\
  !*** ./node_modules/@floating-ui/utils/dist/floating-ui.utils.dom.mjs ***!
  \************************************************************************/
/***/ ((__unused_webpack___webpack_module__, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "getComputedStyle": () => (/* binding */ getComputedStyle),
/* harmony export */   "getContainingBlock": () => (/* binding */ getContainingBlock),
/* harmony export */   "getDocumentElement": () => (/* binding */ getDocumentElement),
/* harmony export */   "getFrameElement": () => (/* binding */ getFrameElement),
/* harmony export */   "getNearestOverflowAncestor": () => (/* binding */ getNearestOverflowAncestor),
/* harmony export */   "getNodeName": () => (/* binding */ getNodeName),
/* harmony export */   "getNodeScroll": () => (/* binding */ getNodeScroll),
/* harmony export */   "getOverflowAncestors": () => (/* binding */ getOverflowAncestors),
/* harmony export */   "getParentNode": () => (/* binding */ getParentNode),
/* harmony export */   "getWindow": () => (/* binding */ getWindow),
/* harmony export */   "isContainingBlock": () => (/* binding */ isContainingBlock),
/* harmony export */   "isElement": () => (/* binding */ isElement),
/* harmony export */   "isHTMLElement": () => (/* binding */ isHTMLElement),
/* harmony export */   "isLastTraversableNode": () => (/* binding */ isLastTraversableNode),
/* harmony export */   "isNode": () => (/* binding */ isNode),
/* harmony export */   "isOverflowElement": () => (/* binding */ isOverflowElement),
/* harmony export */   "isShadowRoot": () => (/* binding */ isShadowRoot),
/* harmony export */   "isTableElement": () => (/* binding */ isTableElement),
/* harmony export */   "isTopLayer": () => (/* binding */ isTopLayer),
/* harmony export */   "isWebKit": () => (/* binding */ isWebKit)
/* harmony export */ });
function hasWindow() {
  return typeof window !== 'undefined';
}
function getNodeName(node) {
  if (isNode(node)) {
    return (node.nodeName || '').toLowerCase();
  }
  // Mocked nodes in testing environments may not be instances of Node. By
  // returning `#document` an infinite loop won't occur.
  // https://github.com/floating-ui/floating-ui/issues/2317
  return '#document';
}
function getWindow(node) {
  var _node$ownerDocument;
  return (node == null || (_node$ownerDocument = node.ownerDocument) == null ? void 0 : _node$ownerDocument.defaultView) || window;
}
function getDocumentElement(node) {
  var _ref;
  return (_ref = (isNode(node) ? node.ownerDocument : node.document) || window.document) == null ? void 0 : _ref.documentElement;
}
function isNode(value) {
  if (!hasWindow()) {
    return false;
  }
  return value instanceof Node || value instanceof getWindow(value).Node;
}
function isElement(value) {
  if (!hasWindow()) {
    return false;
  }
  return value instanceof Element || value instanceof getWindow(value).Element;
}
function isHTMLElement(value) {
  if (!hasWindow()) {
    return false;
  }
  return value instanceof HTMLElement || value instanceof getWindow(value).HTMLElement;
}
function isShadowRoot(value) {
  if (!hasWindow() || typeof ShadowRoot === 'undefined') {
    return false;
  }
  return value instanceof ShadowRoot || value instanceof getWindow(value).ShadowRoot;
}
function isOverflowElement(element) {
  const {
    overflow,
    overflowX,
    overflowY,
    display
  } = getComputedStyle(element);
  return /auto|scroll|overlay|hidden|clip/.test(overflow + overflowY + overflowX) && !['inline', 'contents'].includes(display);
}
function isTableElement(element) {
  return ['table', 'td', 'th'].includes(getNodeName(element));
}
function isTopLayer(element) {
  return [':popover-open', ':modal'].some(selector => {
    try {
      return element.matches(selector);
    } catch (e) {
      return false;
    }
  });
}
function isContainingBlock(elementOrCss) {
  const webkit = isWebKit();
  const css = isElement(elementOrCss) ? getComputedStyle(elementOrCss) : elementOrCss;

  // https://developer.mozilla.org/en-US/docs/Web/CSS/Containing_block#identifying_the_containing_block
  return css.transform !== 'none' || css.perspective !== 'none' || (css.containerType ? css.containerType !== 'normal' : false) || !webkit && (css.backdropFilter ? css.backdropFilter !== 'none' : false) || !webkit && (css.filter ? css.filter !== 'none' : false) || ['transform', 'perspective', 'filter'].some(value => (css.willChange || '').includes(value)) || ['paint', 'layout', 'strict', 'content'].some(value => (css.contain || '').includes(value));
}
function getContainingBlock(element) {
  let currentNode = getParentNode(element);
  while (isHTMLElement(currentNode) && !isLastTraversableNode(currentNode)) {
    if (isContainingBlock(currentNode)) {
      return currentNode;
    } else if (isTopLayer(currentNode)) {
      return null;
    }
    currentNode = getParentNode(currentNode);
  }
  return null;
}
function isWebKit() {
  if (typeof CSS === 'undefined' || !CSS.supports) return false;
  return CSS.supports('-webkit-backdrop-filter', 'none');
}
function isLastTraversableNode(node) {
  return ['html', 'body', '#document'].includes(getNodeName(node));
}
function getComputedStyle(element) {
  return getWindow(element).getComputedStyle(element);
}
function getNodeScroll(element) {
  if (isElement(element)) {
    return {
      scrollLeft: element.scrollLeft,
      scrollTop: element.scrollTop
    };
  }
  return {
    scrollLeft: element.scrollX,
    scrollTop: element.scrollY
  };
}
function getParentNode(node) {
  if (getNodeName(node) === 'html') {
    return node;
  }
  const result =
  // Step into the shadow DOM of the parent of a slotted node.
  node.assignedSlot ||
  // DOM Element detected.
  node.parentNode ||
  // ShadowRoot detected.
  isShadowRoot(node) && node.host ||
  // Fallback.
  getDocumentElement(node);
  return isShadowRoot(result) ? result.host : result;
}
function getNearestOverflowAncestor(node) {
  const parentNode = getParentNode(node);
  if (isLastTraversableNode(parentNode)) {
    return node.ownerDocument ? node.ownerDocument.body : node.body;
  }
  if (isHTMLElement(parentNode) && isOverflowElement(parentNode)) {
    return parentNode;
  }
  return getNearestOverflowAncestor(parentNode);
}
function getOverflowAncestors(node, list, traverseIframes) {
  var _node$ownerDocument2;
  if (list === void 0) {
    list = [];
  }
  if (traverseIframes === void 0) {
    traverseIframes = true;
  }
  const scrollableAncestor = getNearestOverflowAncestor(node);
  const isBody = scrollableAncestor === ((_node$ownerDocument2 = node.ownerDocument) == null ? void 0 : _node$ownerDocument2.body);
  const win = getWindow(scrollableAncestor);
  if (isBody) {
    const frameElement = getFrameElement(win);
    return list.concat(win, win.visualViewport || [], isOverflowElement(scrollableAncestor) ? scrollableAncestor : [], frameElement && traverseIframes ? getOverflowAncestors(frameElement) : []);
  }
  return list.concat(scrollableAncestor, getOverflowAncestors(scrollableAncestor, [], traverseIframes));
}
function getFrameElement(win) {
  return win.parent && Object.getPrototypeOf(win.parent) ? win.frameElement : null;
}




/***/ }),

/***/ "./node_modules/@floating-ui/utils/dist/floating-ui.utils.mjs":
/*!********************************************************************!*\
  !*** ./node_modules/@floating-ui/utils/dist/floating-ui.utils.mjs ***!
  \********************************************************************/
/***/ ((__unused_webpack___webpack_module__, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "alignments": () => (/* binding */ alignments),
/* harmony export */   "clamp": () => (/* binding */ clamp),
/* harmony export */   "createCoords": () => (/* binding */ createCoords),
/* harmony export */   "evaluate": () => (/* binding */ evaluate),
/* harmony export */   "expandPaddingObject": () => (/* binding */ expandPaddingObject),
/* harmony export */   "floor": () => (/* binding */ floor),
/* harmony export */   "getAlignment": () => (/* binding */ getAlignment),
/* harmony export */   "getAlignmentAxis": () => (/* binding */ getAlignmentAxis),
/* harmony export */   "getAlignmentSides": () => (/* binding */ getAlignmentSides),
/* harmony export */   "getAxisLength": () => (/* binding */ getAxisLength),
/* harmony export */   "getExpandedPlacements": () => (/* binding */ getExpandedPlacements),
/* harmony export */   "getOppositeAlignmentPlacement": () => (/* binding */ getOppositeAlignmentPlacement),
/* harmony export */   "getOppositeAxis": () => (/* binding */ getOppositeAxis),
/* harmony export */   "getOppositeAxisPlacements": () => (/* binding */ getOppositeAxisPlacements),
/* harmony export */   "getOppositePlacement": () => (/* binding */ getOppositePlacement),
/* harmony export */   "getPaddingObject": () => (/* binding */ getPaddingObject),
/* harmony export */   "getSide": () => (/* binding */ getSide),
/* harmony export */   "getSideAxis": () => (/* binding */ getSideAxis),
/* harmony export */   "max": () => (/* binding */ max),
/* harmony export */   "min": () => (/* binding */ min),
/* harmony export */   "placements": () => (/* binding */ placements),
/* harmony export */   "rectToClientRect": () => (/* binding */ rectToClientRect),
/* harmony export */   "round": () => (/* binding */ round),
/* harmony export */   "sides": () => (/* binding */ sides)
/* harmony export */ });
/**
 * Custom positioning reference element.
 * @see https://floating-ui.com/docs/virtual-elements
 */

const sides = ['top', 'right', 'bottom', 'left'];
const alignments = ['start', 'end'];
const placements = /*#__PURE__*/sides.reduce((acc, side) => acc.concat(side, side + "-" + alignments[0], side + "-" + alignments[1]), []);
const min = Math.min;
const max = Math.max;
const round = Math.round;
const floor = Math.floor;
const createCoords = v => ({
  x: v,
  y: v
});
const oppositeSideMap = {
  left: 'right',
  right: 'left',
  bottom: 'top',
  top: 'bottom'
};
const oppositeAlignmentMap = {
  start: 'end',
  end: 'start'
};
function clamp(start, value, end) {
  return max(start, min(value, end));
}
function evaluate(value, param) {
  return typeof value === 'function' ? value(param) : value;
}
function getSide(placement) {
  return placement.split('-')[0];
}
function getAlignment(placement) {
  return placement.split('-')[1];
}
function getOppositeAxis(axis) {
  return axis === 'x' ? 'y' : 'x';
}
function getAxisLength(axis) {
  return axis === 'y' ? 'height' : 'width';
}
function getSideAxis(placement) {
  return ['top', 'bottom'].includes(getSide(placement)) ? 'y' : 'x';
}
function getAlignmentAxis(placement) {
  return getOppositeAxis(getSideAxis(placement));
}
function getAlignmentSides(placement, rects, rtl) {
  if (rtl === void 0) {
    rtl = false;
  }
  const alignment = getAlignment(placement);
  const alignmentAxis = getAlignmentAxis(placement);
  const length = getAxisLength(alignmentAxis);
  let mainAlignmentSide = alignmentAxis === 'x' ? alignment === (rtl ? 'end' : 'start') ? 'right' : 'left' : alignment === 'start' ? 'bottom' : 'top';
  if (rects.reference[length] > rects.floating[length]) {
    mainAlignmentSide = getOppositePlacement(mainAlignmentSide);
  }
  return [mainAlignmentSide, getOppositePlacement(mainAlignmentSide)];
}
function getExpandedPlacements(placement) {
  const oppositePlacement = getOppositePlacement(placement);
  return [getOppositeAlignmentPlacement(placement), oppositePlacement, getOppositeAlignmentPlacement(oppositePlacement)];
}
function getOppositeAlignmentPlacement(placement) {
  return placement.replace(/start|end/g, alignment => oppositeAlignmentMap[alignment]);
}
function getSideList(side, isStart, rtl) {
  const lr = ['left', 'right'];
  const rl = ['right', 'left'];
  const tb = ['top', 'bottom'];
  const bt = ['bottom', 'top'];
  switch (side) {
    case 'top':
    case 'bottom':
      if (rtl) return isStart ? rl : lr;
      return isStart ? lr : rl;
    case 'left':
    case 'right':
      return isStart ? tb : bt;
    default:
      return [];
  }
}
function getOppositeAxisPlacements(placement, flipAlignment, direction, rtl) {
  const alignment = getAlignment(placement);
  let list = getSideList(getSide(placement), direction === 'start', rtl);
  if (alignment) {
    list = list.map(side => side + "-" + alignment);
    if (flipAlignment) {
      list = list.concat(list.map(getOppositeAlignmentPlacement));
    }
  }
  return list;
}
function getOppositePlacement(placement) {
  return placement.replace(/left|right|bottom|top/g, side => oppositeSideMap[side]);
}
function expandPaddingObject(padding) {
  return {
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
    ...padding
  };
}
function getPaddingObject(padding) {
  return typeof padding !== 'number' ? expandPaddingObject(padding) : {
    top: padding,
    right: padding,
    bottom: padding,
    left: padding
  };
}
function rectToClientRect(rect) {
  const {
    x,
    y,
    width,
    height
  } = rect;
  return {
    width,
    height,
    top: y,
    left: x,
    right: x + width,
    bottom: y + height,
    x,
    y
  };
}




/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		var cachedModule = __webpack_module_cache__[moduleId];
/******/ 		if (cachedModule !== undefined) {
/******/ 			return cachedModule.exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId](module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/************************************************************************/
var __webpack_exports__ = {};
// This entry need to be wrapped in an IIFE because it need to be isolated against other modules in the chunk.
(() => {
var exports = __webpack_exports__;
/*!********************!*\
  !*** ./js/main.ts ***!
  \********************/

exports.__esModule = true;
var alpinejs_1 = __webpack_require__(/*! alpinejs */ "./node_modules/alpinejs/dist/module.esm.js");
var filteredOverview_1 = __webpack_require__(/*! ./filteredOverview */ "./js/filteredOverview.ts");
var copyToClipboardButton_1 = __webpack_require__(/*! ./copyToClipboardButton */ "./js/copyToClipboardButton.ts");
var modalButton_1 = __webpack_require__(/*! ./modalButton */ "./js/modalButton.ts");
var classHover_1 = __webpack_require__(/*! ./classHover */ "./js/classHover.ts");
var ajaxForm_1 = __webpack_require__(/*! ./ajaxForm */ "./js/ajaxForm.ts");
var tooltip_1 = __webpack_require__(/*! ./tooltip */ "./js/tooltip.ts");
var serverSideOverview_1 = __webpack_require__(/*! ./serverSideOverview */ "./js/serverSideOverview.ts");
var cursorImage_1 = __webpack_require__(/*! ./cursorImage */ "./js/cursorImage.ts");
var importModal_1 = __webpack_require__(/*! ./components/importModal */ "./js/components/importModal.ts");
var toggler_1 = __webpack_require__(/*! ./utils/toggler */ "./js/utils/toggler.ts");
var anchorHover_1 = __webpack_require__(/*! ./anchorHover */ "./js/anchorHover.ts");
var listManager_1 = __webpack_require__(/*! ./components/listManager */ "./js/components/listManager.ts");
var deckCollection_1 = __webpack_require__(/*! ./deckCollection */ "./js/deckCollection.ts");
window.Alpine = alpinejs_1["default"];
alpinejs_1["default"].data('filteredOverview', filteredOverview_1["default"]);
alpinejs_1["default"].data('serverSideOverview', serverSideOverview_1["default"]);
alpinejs_1["default"].data("importModal", importModal_1["default"]);
alpinejs_1["default"].start();
(0, copyToClipboardButton_1["default"])();
(0, modalButton_1["default"])();
(0, classHover_1["default"])();
(0, ajaxForm_1["default"])();
(0, tooltip_1["default"])();
(0, cursorImage_1["default"])();
(0, toggler_1["default"])();
(0, anchorHover_1["default"])();
(0, listManager_1["default"])();
(0, deckCollection_1["default"])();

})();

/******/ })()
;
//# sourceMappingURL=data:application/json;charset=utf-8;base64,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QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBLHVDQUF1QztBQUN2QztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsR0FBRztBQUNIOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsQ0FBQztBQUNEO0FBQ0E7QUFDQTtBQUNBLEtBQUs7QUFDTCxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0wsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0Esa0NBQWtDLFFBQVE7QUFDMUM7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBO0FBQ0E7QUFDQSxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0wsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSTtBQUNKO0FBQ0EsSUFBSTtBQUNKO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBLENBQUM7QUFDRDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsTUFBTTtBQUNOO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0EsOEJBQThCLDZCQUE2QixHQUFHLG9CQUFvQjtBQUNsRjtBQUNBO0FBQ0E7QUFDQTtBQUNBLElBQUk7QUFDSjtBQUNBO0FBQ0EsQ0FBQztBQUNEO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0w7QUFDQTtBQUNBLEtBQUs7QUFDTDtBQUNBO0FBQ0EsS0FBSztBQUNMO0FBQ0E7QUFDQSxLQUFLO0FBQ0w7QUFDQTtBQUNBLEtBQUs7QUFDTDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSw2QkFBNkIsV0FBVztBQUN4QztBQUNBO0FBQ0E7QUFDQTtBQUNBLDBCQUEwQixXQUFXO0FBQ3JDO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSw2QkFBNkIsWUFBWTtBQUN6QztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsMEJBQTBCLFdBQVc7QUFDckM7QUFDQTtBQUNBO0FBQ0Esb0VBQW9FO0FBQ3BFO0FBQ0E7QUFDQSxjQUFjLDZEQUE2RDtBQUMzRSxjQUFjLDZEQUE2RDtBQUMzRTtBQUNBLE9BQU87QUFDUCxPQUFPO0FBQ1A7QUFDQTtBQUNBO0FBQ0E7QUFDQSxTQUFTO0FBQ1QsT0FBTztBQUNQO0FBQ0EsT0FBTztBQUNQLE9BQU87QUFDUDtBQUNBO0FBQ0E7QUFDQTtBQUNBLFNBQVM7QUFDVDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxNQUFNO0FBQ047QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsS0FBSztBQUNMLG1EQUFtRCxnQ0FBZ0M7QUFDbkYsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLE1BQU07QUFDTjtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLFNBQVM7QUFDVCxPQUFPO0FBQ1A7QUFDQSxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxzQ0FBc0MsNEJBQTRCLElBQUk7QUFDdEUsQ0FBQztBQUNELENBQUM7QUFDRDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsS0FBSztBQUNMO0FBQ0E7QUFDQSxLQUFLO0FBQ0w7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0w7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEtBQUs7QUFDTCxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0w7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsS0FBSztBQUNMO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0w7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsT0FBTztBQUNQO0FBQ0E7QUFDQTtBQUNBLEtBQUs7QUFDTCxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQSxDQUFDO0FBQ0Q7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEtBQUs7QUFDTDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0EsZ0NBQWdDO0FBQ2hDO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSTtBQUNKO0FBQ0E7QUFDQSxNQUFNO0FBQ047QUFDQSxNQUFNO0FBQ047QUFDQTtBQUNBLFFBQVE7QUFDUjtBQUNBO0FBQ0E7QUFDQSxJQUFJO0FBQ0o7QUFDQSxJQUFJO0FBQ0o7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSTtBQUNKO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQSxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxJQUFJO0FBQ0o7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEtBQUs7QUFDTCxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsZ0VBQWdFLFlBQVk7QUFDNUU7QUFDQTtBQUNBO0FBQ0E7QUFDQSx3QkFBd0IsZUFBZTtBQUN2QyxtQkFBbUIsZ0JBQWdCO0FB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O0FBQ0E7QUFDQTtBQUNBLE9BQU87QUFDUDtBQUNBO0FBQ0E7QUFDQSxNQUFNO0FBQ047QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxxQ0FBcUMsT0FBTztBQUM1QztBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsTUFBTTtBQUNOO0FBQ0E7QUFDQTtBQUNBLENBQUM7QUFDRDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsQ0FBQztBQUNEO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsTUFBTTtBQUNOO0FBQ0EsTUFBTTtBQUNOO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxRQUFRO0FBQ1I7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsUUFBUSxJQUFJO0FBQ1osNENBQTRDLFlBQVk7QUFDeEQ7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBLFFBQVEsSUFBSTtBQUNaLCtDQUErQyxZQUFZO0FBQzNEO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsdUNBQXVDO0FBQ3ZDO0FBQ0E7QUFDQSxDQUFDO0FBQ0QsdUNBQXVDO0FBQ3ZDO0FBQ0EsQ0FBQztBQUNEO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLFNBQVMsV0FBVztBQUNwQjtBQUNBO0FBQ0E7QUFDQSxJQUFJO0FBQ0o7QUFDQSxJQUFJO0FBQ0o7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLFNBQVMsc0JBQXNCO0FBQy9CO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSSxTQUFTLElBQUk7QUFDakI7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSTtBQUNKO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLFNBQVMsc0JBQXNCO0FBQy9CO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSSxTQUFTLElBQUk7QUFDakI7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0Esb0JBQW9CLEtBQUksdURBQXVELENBQU07QUFDckY7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsS0FBSztBQUNMO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxlQUFlLGFBQWE7QUFDNUIsdUJBQXVCLGFBQWE7QUFDcEM7QUFDQTtBQUNBO0FBQ0EsT0FBTztBQUNQO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxRQUFRLElBQUk7QUFDWix1Q0FBdUMsUUFBUTtBQUMvQyxzQkFBc0Isa0JBQWtCLFlBQVksSUFBSTtBQUN4RDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQSxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxHQUFHO0FBQ0g7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEdBQUc7QUFDSDtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQSxHQUFHO0FBQ0g7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsQ0FBQztBQUNEO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxNQUFNO0FBQ047QUFDQSxNQUFNO0FBQ047QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsNkJBQTZCLE1BQU0sZ0VBQWdFLGlDQUFpQztBQUNwSTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLFFBQVEsSUFBSTtBQUNaLHFEQUFxRCxlQUFlO0FBQ3BFO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBOztBQUVBO0FBQ0E7O0FBRUE7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QTtBQUNBO0FBQ0EsT0FBTztBQUNQO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFc0g7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7QUNoaEN0SDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSTtBQUNKO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLE1BQU07QUFDTjtBQUNBO0FBQ0EsR0FBRztBQUNIO0FBQ0E7QUFDQTtBQUNBOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxNQUFNO0FBQ047QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRWdWOzs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7OztBQ3hKaFY7QUFDQTtBQUNBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxDQUFDO0FBQ0Q7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsSUFBSTtBQUNKO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRXlXOzs7Ozs7O1VDekl6VztVQUNBOztVQUVBO1VBQ0E7VUFDQTtVQUNBO1VBQ0E7VUFDQTtVQUNBO1VBQ0E7VUFDQTtVQUNBO1VBQ0E7VUFDQTtVQUNBOztVQUVBO1VBQ0E7O1VBRUE7VUFDQTtVQUNBOzs7OztXQ3RCQTtXQUNBO1dBQ0E7V0FDQTtXQUNBLHlDQUF5Qyx3Q0FBd0M7V0FDakY7V0FDQTtXQUNBOzs7OztXQ1BBOzs7OztXQ0FBO1dBQ0E7V0FDQTtXQUNBLHVEQUF1RCxpQkFBaUI7V0FDeEU7V0FDQSxnREFBZ0QsYUFBYTtXQUM3RDs7Ozs7Ozs7Ozs7OztBQ05BLG1HQUE2QjtBQUM3QixtR0FBa0Q7QUFDbEQsa0hBQTREO0FBQzVELG9GQUF3QztBQUN4QyxpRkFBc0M7QUFDdEMsMkVBQWtDO0FBQ2xDLHdFQUFnQztBQUNoQyx5R0FBc0Q7QUFDdEQsb0ZBQXdDO0FBQ3hDLDBHQUFtRDtBQUNuRCxvRkFBc0M7QUFDdEMsb0ZBQXdDO0FBQ3hDLDBHQUFtRDtBQUNuRCw2RkFBOEM7QUFFN0MsTUFBYyxDQUFDLE1BQU0sR0FBRyxxQkFBTSxDQUFDO0FBRWhDLHFCQUFNLENBQUMsSUFBSSxDQUFDLGtCQUFrQixFQUFFLDZCQUFnQixDQUFDLENBQUM7QUFDbEQscUJBQU0sQ0FBQyxJQUFJLENBQUMsb0JBQW9CLEVBQUUsK0JBQWtCLENBQUMsQ0FBQztBQUN0RCxxQkFBTSxDQUFDLElBQUksQ0FBQyxhQUFhLEVBQUUsd0JBQVcsQ0FBQyxDQUFDO0FBRXhDLHFCQUFNLENBQUMsS0FBSyxFQUFFO0FBRWQsc0NBQXFCLEdBQUUsQ0FBQztBQUN4Qiw0QkFBVyxHQUFFLENBQUM7QUFDZCwyQkFBVSxHQUFFLENBQUM7QUFDYix5QkFBUSxHQUFFLENBQUM7QUFDWCx3QkFBTyxHQUFFLENBQUM7QUFDViw0QkFBVyxHQUFFLENBQUM7QUFDZCx3QkFBTyxHQUFFLENBQUM7QUFDViw0QkFBVyxHQUFFLENBQUM7QUFDZCw0QkFBVyxHQUFFLENBQUM7QUFDZCwrQkFBYyxHQUFFLENBQUMiLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly9za3l0ZWFyaG9yZGUuc3RhdGljLy4vbm9kZV9tb2R1bGVzL0BmbG9hdGluZy11aS9kb20vZGlzdC9mbG9hdGluZy11aS5kb20uZXNtLmpzIiwid2VicGFjazovL3NreXRlYXJob3JkZS5zdGF0aWMvLi9ub2RlX21vZHVsZXMvYWxwaW5lanMvZGlzdC9tb2R1bGUuZXNtLmpzIiwid2VicGFjazovL3NreXRlYXJob3JkZS5zdGF0aWMvLi9qcy9hamF4Rm9ybS50cyIsIndlYnBhY2s6Ly9za3l0ZWFyaG9yZGUuc3RhdGljLy4vanMvYW5jaG9ySG92ZXIudHMiLCJ3ZWJwYWNrOi8vc2t5dGVhcmhvcmRlLnN0YXRpYy8uL2pzL2NsYXNzSG92ZXIudHMiLCJ3ZWJwYWNrOi8vc2t5dGVhcmhvcmRlLnN0YXRpYy8uL2pzL2NvbXBvbmVudHMvaW1wb3J0TW9kYWwudHMiLCJ3ZWJwYWNrOi8vc2t5dGVhcmhvcmRlLnN0YXRpYy8uL2pzL2NvbXBvbmVudHMvbGlzdE1hbmFnZXIudHMiLCJ3ZWJwYWNrOi8vc2t5dGVhcmhvcmRlLnN0YXRpYy8uL2pzL2NvcHlUb0NsaXBib2FyZEJ1dHRvbi50cyIsIndlYnBhY2s6Ly9za3l0ZWFyaG9yZGUuc3RhdGljLy4vanMvY3Vyc29ySW1hZ2UudHMiLCJ3ZWJwYWNrOi8vc2t5dGVhcmhvcmRlLnN0YXRpYy8uL2pzL2RlY2tDb2xsZWN0aW9uLnRzIiwid2VicGFjazovL3NreXRlYXJob3JkZS5zdGF0aWMvLi9qcy9maWx0ZXJlZE92ZXJ2aWV3LnRzIiwid2VicGFjazovL3NreXRlYXJob3JkZS5zdGF0aWMvLi9qcy9tb2RhbEJ1dHRvbi50cyIsIndlYnBhY2s6Ly9za3l0ZWFyaG9yZGUuc3RhdGljLy4vanMvc2VydmVyU2lkZU92ZXJ2aWV3LnRzIiwid2VicGFjazovL3NreXRlYXJob3JkZS5zdGF0aWMvLi9qcy90b29sdGlwLnRzIiwid2VicGFjazovL3NreXRlYXJob3JkZS5zdGF0aWMvLi9qcy91dGlscy90b2dnbGVyLnRzIiwid2VicGFjazovL3NreXRlYXJob3JkZS5zdGF0aWMvLi9ub2RlX21vZHVsZXMvQGZsb2F0aW5nLXVpL2NvcmUvZGlzdC9mbG9hdGluZy11aS5jb3JlLm1qcyIsIndlYnBhY2s6Ly9za3l0ZWFyaG9yZGUuc3RhdGljLy4vbm9kZV9tb2R1bGVzL0BmbG9hdGluZy11aS91dGlscy9kaXN0L2Zsb2F0aW5nLXVpLnV0aWxzLmRvbS5tanMiLCJ3ZWJwYWNrOi8vc2t5dGVhcmhvcmRlLnN0YXRpYy8uL25vZGVfbW9kdWxlcy9AZmxvYXRpbmctdWkvdXRpbHMvZGlzdC9mbG9hdGluZy11aS51dGlscy5tanMiLCJ3ZWJwYWNrOi8vc2t5dGVhcmhvcmRlLnN0YXRpYy93ZWJwYWNrL2Jvb3RzdHJhcCIsIndlYnBhY2s6Ly9za3l0ZWFyaG9yZGUuc3RhdGljL3dlYnBhY2svcnVudGltZS9kZWZpbmUgcHJvcGVydHkgZ2V0dGVycyIsIndlYnBhY2s6Ly9za3l0ZWFyaG9yZGUuc3RhdGljL3dlYnBhY2svcnVudGltZS9oYXNPd25Qcm9wZXJ0eSBzaG9ydGhhbmQiLCJ3ZWJwYWNrOi8vc2t5dGVhcmhvcmRlLnN0YXRpYy93ZWJwYWNrL3J1bnRpbWUvbWFrZSBuYW1lc3BhY2Ugb2JqZWN0Iiwid2VicGFjazovL3NreXRlYXJob3JkZS5zdGF0aWMvLi9qcy9tYWluLnRzIl0sInNvdXJjZXNDb250ZW50IjpbImltcG9ydCB7IHJlY3RUb0NsaWVudFJlY3QsIGRldGVjdE92ZXJmbG93IGFzIGRldGVjdE92ZXJmbG93JDEsIG9mZnNldCBhcyBvZmZzZXQkMSwgYXV0b1BsYWNlbWVudCBhcyBhdXRvUGxhY2VtZW50JDEsIHNoaWZ0IGFzIHNoaWZ0JDEsIGZsaXAgYXMgZmxpcCQxLCBzaXplIGFzIHNpemUkMSwgaGlkZSBhcyBoaWRlJDEsIGFycm93IGFzIGFycm93JDEsIGlubGluZSBhcyBpbmxpbmUkMSwgbGltaXRTaGlmdCBhcyBsaW1pdFNoaWZ0JDEsIGNvbXB1dGVQb3NpdGlvbiBhcyBjb21wdXRlUG9zaXRpb24kMSB9IGZyb20gJ0BmbG9hdGluZy11aS9jb3JlJztcbmltcG9ydCB7IHJvdW5kLCBjcmVhdGVDb29yZHMsIG1heCwgbWluLCBmbG9vciB9IGZyb20gJ0BmbG9hdGluZy11aS91dGlscyc7XG5pbXBvcnQgeyBnZXRDb21wdXRlZFN0eWxlLCBpc0hUTUxFbGVtZW50LCBpc0VsZW1lbnQsIGdldFdpbmRvdywgaXNXZWJLaXQsIGdldEZyYW1lRWxlbWVudCwgZ2V0Tm9kZVNjcm9sbCwgZ2V0RG9jdW1lbnRFbGVtZW50LCBpc1RvcExheWVyLCBnZXROb2RlTmFtZSwgaXNPdmVyZmxvd0VsZW1lbnQsIGdldE92ZXJmbG93QW5jZXN0b3JzLCBnZXRQYXJlbnROb2RlLCBpc0xhc3RUcmF2ZXJzYWJsZU5vZGUsIGlzQ29udGFpbmluZ0Jsb2NrLCBpc1RhYmxlRWxlbWVudCwgZ2V0Q29udGFpbmluZ0Jsb2NrIH0gZnJvbSAnQGZsb2F0aW5nLXVpL3V0aWxzL2RvbSc7XG5leHBvcnQgeyBnZXRPdmVyZmxvd0FuY2VzdG9ycyB9IGZyb20gJ0BmbG9hdGluZy11aS91dGlscy9kb20nO1xuXG5mdW5jdGlvbiBnZXRDc3NEaW1lbnNpb25zKGVsZW1lbnQpIHtcbiAgY29uc3QgY3NzID0gZ2V0Q29tcHV0ZWRTdHlsZShlbGVtZW50KTtcbiAgLy8gSW4gdGVzdGluZyBlbnZpcm9ubWVudHMsIHRoZSBgd2lkdGhgIGFuZCBgaGVpZ2h0YCBwcm9wZXJ0aWVzIGFyZSBlbXB0eVxuICAvLyBzdHJpbmdzIGZvciBTVkcgZWxlbWVudHMsIHJldHVybmluZyBOYU4uIEZhbGxiYWNrIHRvIGAwYCBpbiB0aGlzIGNhc2UuXG4gIGxldCB3aWR0aCA9IHBhcnNlRmxvYXQoY3NzLndpZHRoKSB8fCAwO1xuICBsZXQgaGVpZ2h0ID0gcGFyc2VGbG9hdChjc3MuaGVpZ2h0KSB8fCAwO1xuICBjb25zdCBoYXNPZmZzZXQgPSBpc0hUTUxFbGVtZW50KGVsZW1lbnQpO1xuICBjb25zdCBvZmZzZXRXaWR0aCA9IGhhc09mZnNldCA/IGVsZW1lbnQub2Zmc2V0V2lkdGggOiB3aWR0aDtcbiAgY29uc3Qgb2Zmc2V0SGVpZ2h0ID0gaGFzT2Zmc2V0ID8gZWxlbWVudC5vZmZzZXRIZWlnaHQgOiBoZWlnaHQ7XG4gIGNvbnN0IHNob3VsZEZhbGxiYWNrID0gcm91bmQod2lkdGgpICE9PSBvZmZzZXRXaWR0aCB8fCByb3VuZChoZWlnaHQpICE9PSBvZmZzZXRIZWlnaHQ7XG4gIGlmIChzaG91bGRGYWxsYmFjaykge1xuICAgIHdpZHRoID0gb2Zmc2V0V2lkdGg7XG4gICAgaGVpZ2h0ID0gb2Zmc2V0SGVpZ2h0O1xuICB9XG4gIHJldHVybiB7XG4gICAgd2lkdGgsXG4gICAgaGVpZ2h0LFxuICAgICQ6IHNob3VsZEZhbGxiYWNrXG4gIH07XG59XG5cbmZ1bmN0aW9uIHVud3JhcEVsZW1lbnQoZWxlbWVudCkge1xuICByZXR1cm4gIWlzRWxlbWVudChlbGVtZW50KSA/IGVsZW1lbnQuY29udGV4dEVsZW1lbnQgOiBlbGVtZW50O1xufVxuXG5mdW5jdGlvbiBnZXRTY2FsZShlbGVtZW50KSB7XG4gIGNvbnN0IGRvbUVsZW1lbnQgPSB1bndyYXBFbGVtZW50KGVsZW1lbnQpO1xuICBpZiAoIWlzSFRNTEVsZW1lbnQoZG9tRWxlbWVudCkpIHtcbiAgICByZXR1cm4gY3JlYXRlQ29vcmRzKDEpO1xuICB9XG4gIGNvbnN0IHJlY3QgPSBkb21FbGVtZW50LmdldEJvdW5kaW5nQ2xpZW50UmVjdCgpO1xuICBjb25zdCB7XG4gICAgd2lkdGgsXG4gICAgaGVpZ2h0LFxuICAgICRcbiAgfSA9IGdldENzc0RpbWVuc2lvbnMoZG9tRWxlbWVudCk7XG4gIGxldCB4ID0gKCQgPyByb3VuZChyZWN0LndpZHRoKSA6IHJlY3Qud2lkdGgpIC8gd2lkdGg7XG4gIGxldCB5ID0gKCQgPyByb3VuZChyZWN0LmhlaWdodCkgOiByZWN0LmhlaWdodCkgLyBoZWlnaHQ7XG5cbiAgLy8gMCwgTmFOLCBvciBJbmZpbml0eSBzaG91bGQgYWx3YXlzIGZhbGxiYWNrIHRvIDEuXG5cbiAgaWYgKCF4IHx8ICFOdW1iZXIuaXNGaW5pdGUoeCkpIHtcbiAgICB4ID0gMTtcbiAgfVxuICBpZiAoIXkgfHwgIU51bWJlci5pc0Zpbml0ZSh5KSkge1xuICAgIHkgPSAxO1xuICB9XG4gIHJldHVybiB7XG4gICAgeCxcbiAgICB5XG4gIH07XG59XG5cbmNvbnN0IG5vT2Zmc2V0cyA9IC8qI19fUFVSRV9fKi9jcmVhdGVDb29yZHMoMCk7XG5mdW5jdGlvbiBnZXRWaXN1YWxPZmZzZXRzKGVsZW1lbnQpIHtcbiAgY29uc3Qgd2luID0gZ2V0V2luZG93KGVsZW1lbnQpO1xuICBpZiAoIWlzV2ViS2l0KCkgfHwgIXdpbi52aXN1YWxWaWV3cG9ydCkge1xuICAgIHJldHVybiBub09mZnNldHM7XG4gIH1cbiAgcmV0dXJuIHtcbiAgICB4OiB3aW4udmlzdWFsVmlld3BvcnQub2Zmc2V0TGVmdCxcbiAgICB5OiB3aW4udmlzdWFsVmlld3BvcnQub2Zmc2V0VG9wXG4gIH07XG59XG5mdW5jdGlvbiBzaG91bGRBZGRWaXN1YWxPZmZzZXRzKGVsZW1lbnQsIGlzRml4ZWQsIGZsb2F0aW5nT2Zmc2V0UGFyZW50KSB7XG4gIGlmIChpc0ZpeGVkID09PSB2b2lkIDApIHtcbiAgICBpc0ZpeGVkID0gZmFsc2U7XG4gIH1cbiAgaWYgKCFmbG9hdGluZ09mZnNldFBhcmVudCB8fCBpc0ZpeGVkICYmIGZsb2F0aW5nT2Zmc2V0UGFyZW50ICE9PSBnZXRXaW5kb3coZWxlbWVudCkpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cbiAgcmV0dXJuIGlzRml4ZWQ7XG59XG5cbmZ1bmN0aW9uIGdldEJvdW5kaW5nQ2xpZW50UmVjdChlbGVtZW50LCBpbmNsdWRlU2NhbGUsIGlzRml4ZWRTdHJhdGVneSwgb2Zmc2V0UGFyZW50KSB7XG4gIGlmIChpbmNsdWRlU2NhbGUgPT09IHZvaWQgMCkge1xuICAgIGluY2x1ZGVTY2FsZSA9IGZhbHNlO1xuICB9XG4gIGlmIChpc0ZpeGVkU3RyYXRlZ3kgPT09IHZvaWQgMCkge1xuICAgIGlzRml4ZWRTdHJhdGVneSA9IGZhbHNlO1xuICB9XG4gIGNvbnN0IGNsaWVudFJlY3QgPSBlbGVtZW50LmdldEJvdW5kaW5nQ2xpZW50UmVjdCgpO1xuICBjb25zdCBkb21FbGVtZW50ID0gdW53cmFwRWxlbWVudChlbGVtZW50KTtcbiAgbGV0IHNjYWxlID0gY3JlYXRlQ29vcmRzKDEpO1xuICBpZiAoaW5jbHVkZVNjYWxlKSB7XG4gICAgaWYgKG9mZnNldFBhcmVudCkge1xuICAgICAgaWYgKGlzRWxlbWVudChvZmZzZXRQYXJlbnQpKSB7XG4gICAgICAgIHNjYWxlID0gZ2V0U2NhbGUob2Zmc2V0UGFyZW50KTtcbiAgICAgIH1cbiAgICB9IGVsc2Uge1xuICAgICAgc2NhbGUgPSBnZXRTY2FsZShlbGVtZW50KTtcbiAgICB9XG4gIH1cbiAgY29uc3QgdmlzdWFsT2Zmc2V0cyA9IHNob3VsZEFkZFZpc3VhbE9mZnNldHMoZG9tRWxlbWVudCwgaXNGaXhlZFN0cmF0ZWd5LCBvZmZzZXRQYXJlbnQpID8gZ2V0VmlzdWFsT2Zmc2V0cyhkb21FbGVtZW50KSA6IGNyZWF0ZUNvb3JkcygwKTtcbiAgbGV0IHggPSAoY2xpZW50UmVjdC5sZWZ0ICsgdmlzdWFsT2Zmc2V0cy54KSAvIHNjYWxlLng7XG4gIGxldCB5ID0gKGNsaWVudFJlY3QudG9wICsgdmlzdWFsT2Zmc2V0cy55KSAvIHNjYWxlLnk7XG4gIGxldCB3aWR0aCA9IGNsaWVudFJlY3Qud2lkdGggLyBzY2FsZS54O1xuICBsZXQgaGVpZ2h0ID0gY2xpZW50UmVjdC5oZWlnaHQgLyBzY2FsZS55O1xuICBpZiAoZG9tRWxlbWVudCkge1xuICAgIGNvbnN0IHdpbiA9IGdldFdpbmRvdyhkb21FbGVtZW50KTtcbiAgICBjb25zdCBvZmZzZXRXaW4gPSBvZmZzZXRQYXJlbnQgJiYgaXNFbGVtZW50KG9mZnNldFBhcmVudCkgPyBnZXRXaW5kb3cob2Zmc2V0UGFyZW50KSA6IG9mZnNldFBhcmVudDtcbiAgICBsZXQgY3VycmVudFdpbiA9IHdpbjtcbiAgICBsZXQgY3VycmVudElGcmFtZSA9IGdldEZyYW1lRWxlbWVudChjdXJyZW50V2luKTtcbiAgICB3aGlsZSAoY3VycmVudElGcmFtZSAmJiBvZmZzZXRQYXJlbnQgJiYgb2Zmc2V0V2luICE9PSBjdXJyZW50V2luKSB7XG4gICAgICBjb25zdCBpZnJhbWVTY2FsZSA9IGdldFNjYWxlKGN1cnJlbnRJRnJhbWUpO1xuICAgICAgY29uc3QgaWZyYW1lUmVjdCA9IGN1cnJlbnRJRnJhbWUuZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCk7XG4gICAgICBjb25zdCBjc3MgPSBnZXRDb21wdXRlZFN0eWxlKGN1cnJlbnRJRnJhbWUpO1xuICAgICAgY29uc3QgbGVmdCA9IGlmcmFtZVJlY3QubGVmdCArIChjdXJyZW50SUZyYW1lLmNsaWVudExlZnQgKyBwYXJzZUZsb2F0KGNzcy5wYWRkaW5nTGVmdCkpICogaWZyYW1lU2NhbGUueDtcbiAgICAgIGNvbnN0IHRvcCA9IGlmcmFtZVJlY3QudG9wICsgKGN1cnJlbnRJRnJhbWUuY2xpZW50VG9wICsgcGFyc2VGbG9hdChjc3MucGFkZGluZ1RvcCkpICogaWZyYW1lU2NhbGUueTtcbiAgICAgIHggKj0gaWZyYW1lU2NhbGUueDtcbiAgICAgIHkgKj0gaWZyYW1lU2NhbGUueTtcbiAgICAgIHdpZHRoICo9IGlmcmFtZVNjYWxlLng7XG4gICAgICBoZWlnaHQgKj0gaWZyYW1lU2NhbGUueTtcbiAgICAgIHggKz0gbGVmdDtcbiAgICAgIHkgKz0gdG9wO1xuICAgICAgY3VycmVudFdpbiA9IGdldFdpbmRvdyhjdXJyZW50SUZyYW1lKTtcbiAgICAgIGN1cnJlbnRJRnJhbWUgPSBnZXRGcmFtZUVsZW1lbnQoY3VycmVudFdpbik7XG4gICAgfVxuICB9XG4gIHJldHVybiByZWN0VG9DbGllbnRSZWN0KHtcbiAgICB3aWR0aCxcbiAgICBoZWlnaHQsXG4gICAgeCxcbiAgICB5XG4gIH0pO1xufVxuXG4vLyBJZiA8aHRtbD4gaGFzIGEgQ1NTIHdpZHRoIGdyZWF0ZXIgdGhhbiB0aGUgdmlld3BvcnQsIHRoZW4gdGhpcyB3aWxsIGJlXG4vLyBpbmNvcnJlY3QgZm9yIFJUTC5cbmZ1bmN0aW9uIGdldFdpbmRvd1Njcm9sbEJhclgoZWxlbWVudCwgcmVjdCkge1xuICBjb25zdCBsZWZ0U2Nyb2xsID0gZ2V0Tm9kZVNjcm9sbChlbGVtZW50KS5zY3JvbGxMZWZ0O1xuICBpZiAoIXJlY3QpIHtcbiAgICByZXR1cm4gZ2V0Qm91bmRpbmdDbGllbnRSZWN0KGdldERvY3VtZW50RWxlbWVudChlbGVtZW50KSkubGVmdCArIGxlZnRTY3JvbGw7XG4gIH1cbiAgcmV0dXJuIHJlY3QubGVmdCArIGxlZnRTY3JvbGw7XG59XG5cbmZ1bmN0aW9uIGdldEhUTUxPZmZzZXQoZG9jdW1lbnRFbGVtZW50LCBzY3JvbGwsIGlnbm9yZVNjcm9sbGJhclgpIHtcbiAgaWYgKGlnbm9yZVNjcm9sbGJhclggPT09IHZvaWQgMCkge1xuICAgIGlnbm9yZVNjcm9sbGJhclggPSBmYWxzZTtcbiAgfVxuICBjb25zdCBodG1sUmVjdCA9IGRvY3VtZW50RWxlbWVudC5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKTtcbiAgY29uc3QgeCA9IGh0bWxSZWN0LmxlZnQgKyBzY3JvbGwuc2Nyb2xsTGVmdCAtIChpZ25vcmVTY3JvbGxiYXJYID8gMCA6XG4gIC8vIFJUTCA8Ym9keT4gc2Nyb2xsYmFyLlxuICBnZXRXaW5kb3dTY3JvbGxCYXJYKGRvY3VtZW50RWxlbWVudCwgaHRtbFJlY3QpKTtcbiAgY29uc3QgeSA9IGh0bWxSZWN0LnRvcCArIHNjcm9sbC5zY3JvbGxUb3A7XG4gIHJldHVybiB7XG4gICAgeCxcbiAgICB5XG4gIH07XG59XG5cbmZ1bmN0aW9uIGNvbnZlcnRPZmZzZXRQYXJlbnRSZWxhdGl2ZVJlY3RUb1ZpZXdwb3J0UmVsYXRpdmVSZWN0KF9yZWYpIHtcbiAgbGV0IHtcbiAgICBlbGVtZW50cyxcbiAgICByZWN0LFxuICAgIG9mZnNldFBhcmVudCxcbiAgICBzdHJhdGVneVxuICB9ID0gX3JlZjtcbiAgY29uc3QgaXNGaXhlZCA9IHN0cmF0ZWd5ID09PSAnZml4ZWQnO1xuICBjb25zdCBkb2N1bWVudEVsZW1lbnQgPSBnZXREb2N1bWVudEVsZW1lbnQob2Zmc2V0UGFyZW50KTtcbiAgY29uc3QgdG9wTGF5ZXIgPSBlbGVtZW50cyA/IGlzVG9wTGF5ZXIoZWxlbWVudHMuZmxvYXRpbmcpIDogZmFsc2U7XG4gIGlmIChvZmZzZXRQYXJlbnQgPT09IGRvY3VtZW50RWxlbWVudCB8fCB0b3BMYXllciAmJiBpc0ZpeGVkKSB7XG4gICAgcmV0dXJuIHJlY3Q7XG4gIH1cbiAgbGV0IHNjcm9sbCA9IHtcbiAgICBzY3JvbGxMZWZ0OiAwLFxuICAgIHNjcm9sbFRvcDogMFxuICB9O1xuICBsZXQgc2NhbGUgPSBjcmVhdGVDb29yZHMoMSk7XG4gIGNvbnN0IG9mZnNldHMgPSBjcmVhdGVDb29yZHMoMCk7XG4gIGNvbnN0IGlzT2Zmc2V0UGFyZW50QW5FbGVtZW50ID0gaXNIVE1MRWxlbWVudChvZmZzZXRQYXJlbnQpO1xuICBpZiAoaXNPZmZzZXRQYXJlbnRBbkVsZW1lbnQgfHwgIWlzT2Zmc2V0UGFyZW50QW5FbGVtZW50ICYmICFpc0ZpeGVkKSB7XG4gICAgaWYgKGdldE5vZGVOYW1lKG9mZnNldFBhcmVudCkgIT09ICdib2R5JyB8fCBpc092ZXJmbG93RWxlbWVudChkb2N1bWVudEVsZW1lbnQpKSB7XG4gICAgICBzY3JvbGwgPSBnZXROb2RlU2Nyb2xsKG9mZnNldFBhcmVudCk7XG4gICAgfVxuICAgIGlmIChpc0hUTUxFbGVtZW50KG9mZnNldFBhcmVudCkpIHtcbiAgICAgIGNvbnN0IG9mZnNldFJlY3QgPSBnZXRCb3VuZGluZ0NsaWVudFJlY3Qob2Zmc2V0UGFyZW50KTtcbiAgICAgIHNjYWxlID0gZ2V0U2NhbGUob2Zmc2V0UGFyZW50KTtcbiAgICAgIG9mZnNldHMueCA9IG9mZnNldFJlY3QueCArIG9mZnNldFBhcmVudC5jbGllbnRMZWZ0O1xuICAgICAgb2Zmc2V0cy55ID0gb2Zmc2V0UmVjdC55ICsgb2Zmc2V0UGFyZW50LmNsaWVudFRvcDtcbiAgICB9XG4gIH1cbiAgY29uc3QgaHRtbE9mZnNldCA9IGRvY3VtZW50RWxlbWVudCAmJiAhaXNPZmZzZXRQYXJlbnRBbkVsZW1lbnQgJiYgIWlzRml4ZWQgPyBnZXRIVE1MT2Zmc2V0KGRvY3VtZW50RWxlbWVudCwgc2Nyb2xsLCB0cnVlKSA6IGNyZWF0ZUNvb3JkcygwKTtcbiAgcmV0dXJuIHtcbiAgICB3aWR0aDogcmVjdC53aWR0aCAqIHNjYWxlLngsXG4gICAgaGVpZ2h0OiByZWN0LmhlaWdodCAqIHNjYWxlLnksXG4gICAgeDogcmVjdC54ICogc2NhbGUueCAtIHNjcm9sbC5zY3JvbGxMZWZ0ICogc2NhbGUueCArIG9mZnNldHMueCArIGh0bWxPZmZzZXQueCxcbiAgICB5OiByZWN0LnkgKiBzY2FsZS55IC0gc2Nyb2xsLnNjcm9sbFRvcCAqIHNjYWxlLnkgKyBvZmZzZXRzLnkgKyBodG1sT2Zmc2V0LnlcbiAgfTtcbn1cblxuZnVuY3Rpb24gZ2V0Q2xpZW50UmVjdHMoZWxlbWVudCkge1xuICByZXR1cm4gQXJyYXkuZnJvbShlbGVtZW50LmdldENsaWVudFJlY3RzKCkpO1xufVxuXG4vLyBHZXRzIHRoZSBlbnRpcmUgc2l6ZSBvZiB0aGUgc2Nyb2xsYWJsZSBkb2N1bWVudCBhcmVhLCBldmVuIGV4dGVuZGluZyBvdXRzaWRlXG4vLyBvZiB0aGUgYDxodG1sPmAgYW5kIGA8Ym9keT5gIHJlY3QgYm91bmRzIGlmIGhvcml6b250YWxseSBzY3JvbGxhYmxlLlxuZnVuY3Rpb24gZ2V0RG9jdW1lbnRSZWN0KGVsZW1lbnQpIHtcbiAgY29uc3QgaHRtbCA9IGdldERvY3VtZW50RWxlbWVudChlbGVtZW50KTtcbiAgY29uc3Qgc2Nyb2xsID0gZ2V0Tm9kZVNjcm9sbChlbGVtZW50KTtcbiAgY29uc3QgYm9keSA9IGVsZW1lbnQub3duZXJEb2N1bWVudC5ib2R5O1xuICBjb25zdCB3aWR0aCA9IG1heChodG1sLnNjcm9sbFdpZHRoLCBodG1sLmNsaWVudFdpZHRoLCBib2R5LnNjcm9sbFdpZHRoLCBib2R5LmNsaWVudFdpZHRoKTtcbiAgY29uc3QgaGVpZ2h0ID0gbWF4KGh0bWwuc2Nyb2xsSGVpZ2h0LCBodG1sLmNsaWVudEhlaWdodCwgYm9keS5zY3JvbGxIZWlnaHQsIGJvZHkuY2xpZW50SGVpZ2h0KTtcbiAgbGV0IHggPSAtc2Nyb2xsLnNjcm9sbExlZnQgKyBnZXRXaW5kb3dTY3JvbGxCYXJYKGVsZW1lbnQpO1xuICBjb25zdCB5ID0gLXNjcm9sbC5zY3JvbGxUb3A7XG4gIGlmIChnZXRDb21wdXRlZFN0eWxlKGJvZHkpLmRpcmVjdGlvbiA9PT0gJ3J0bCcpIHtcbiAgICB4ICs9IG1heChodG1sLmNsaWVudFdpZHRoLCBib2R5LmNsaWVudFdpZHRoKSAtIHdpZHRoO1xuICB9XG4gIHJldHVybiB7XG4gICAgd2lkdGgsXG4gICAgaGVpZ2h0LFxuICAgIHgsXG4gICAgeVxuICB9O1xufVxuXG5mdW5jdGlvbiBnZXRWaWV3cG9ydFJlY3QoZWxlbWVudCwgc3RyYXRlZ3kpIHtcbiAgY29uc3Qgd2luID0gZ2V0V2luZG93KGVsZW1lbnQpO1xuICBjb25zdCBodG1sID0gZ2V0RG9jdW1lbnRFbGVtZW50KGVsZW1lbnQpO1xuICBjb25zdCB2aXN1YWxWaWV3cG9ydCA9IHdpbi52aXN1YWxWaWV3cG9ydDtcbiAgbGV0IHdpZHRoID0gaHRtbC5jbGllbnRXaWR0aDtcbiAgbGV0IGhlaWdodCA9IGh0bWwuY2xpZW50SGVpZ2h0O1xuICBsZXQgeCA9IDA7XG4gIGxldCB5ID0gMDtcbiAgaWYgKHZpc3VhbFZpZXdwb3J0KSB7XG4gICAgd2lkdGggPSB2aXN1YWxWaWV3cG9ydC53aWR0aDtcbiAgICBoZWlnaHQgPSB2aXN1YWxWaWV3cG9ydC5oZWlnaHQ7XG4gICAgY29uc3QgdmlzdWFsVmlld3BvcnRCYXNlZCA9IGlzV2ViS2l0KCk7XG4gICAgaWYgKCF2aXN1YWxWaWV3cG9ydEJhc2VkIHx8IHZpc3VhbFZpZXdwb3J0QmFzZWQgJiYgc3RyYXRlZ3kgPT09ICdmaXhlZCcpIHtcbiAgICAgIHggPSB2aXN1YWxWaWV3cG9ydC5vZmZzZXRMZWZ0O1xuICAgICAgeSA9IHZpc3VhbFZpZXdwb3J0Lm9mZnNldFRvcDtcbiAgICB9XG4gIH1cbiAgcmV0dXJuIHtcbiAgICB3aWR0aCxcbiAgICBoZWlnaHQsXG4gICAgeCxcbiAgICB5XG4gIH07XG59XG5cbi8vIFJldHVybnMgdGhlIGlubmVyIGNsaWVudCByZWN0LCBzdWJ0cmFjdGluZyBzY3JvbGxiYXJzIGlmIHByZXNlbnQuXG5mdW5jdGlvbiBnZXRJbm5lckJvdW5kaW5nQ2xpZW50UmVjdChlbGVtZW50LCBzdHJhdGVneSkge1xuICBjb25zdCBjbGllbnRSZWN0ID0gZ2V0Qm91bmRpbmdDbGllbnRSZWN0KGVsZW1lbnQsIHRydWUsIHN0cmF0ZWd5ID09PSAnZml4ZWQnKTtcbiAgY29uc3QgdG9wID0gY2xpZW50UmVjdC50b3AgKyBlbGVtZW50LmNsaWVudFRvcDtcbiAgY29uc3QgbGVmdCA9IGNsaWVudFJlY3QubGVmdCArIGVsZW1lbnQuY2xpZW50TGVmdDtcbiAgY29uc3Qgc2NhbGUgPSBpc0hUTUxFbGVtZW50KGVsZW1lbnQpID8gZ2V0U2NhbGUoZWxlbWVudCkgOiBjcmVhdGVDb29yZHMoMSk7XG4gIGNvbnN0IHdpZHRoID0gZWxlbWVudC5jbGllbnRXaWR0aCAqIHNjYWxlLng7XG4gIGNvbnN0IGhlaWdodCA9IGVsZW1lbnQuY2xpZW50SGVpZ2h0ICogc2NhbGUueTtcbiAgY29uc3QgeCA9IGxlZnQgKiBzY2FsZS54O1xuICBjb25zdCB5ID0gdG9wICogc2NhbGUueTtcbiAgcmV0dXJuIHtcbiAgICB3aWR0aCxcbiAgICBoZWlnaHQsXG4gICAgeCxcbiAgICB5XG4gIH07XG59XG5mdW5jdGlvbiBnZXRDbGllbnRSZWN0RnJvbUNsaXBwaW5nQW5jZXN0b3IoZWxlbWVudCwgY2xpcHBpbmdBbmNlc3Rvciwgc3RyYXRlZ3kpIHtcbiAgbGV0IHJlY3Q7XG4gIGlmIChjbGlwcGluZ0FuY2VzdG9yID09PSAndmlld3BvcnQnKSB7XG4gICAgcmVjdCA9IGdldFZpZXdwb3J0UmVjdChlbGVtZW50LCBzdHJhdGVneSk7XG4gIH0gZWxzZSBpZiAoY2xpcHBpbmdBbmNlc3RvciA9PT0gJ2RvY3VtZW50Jykge1xuICAgIHJlY3QgPSBnZXREb2N1bWVudFJlY3QoZ2V0RG9jdW1lbnRFbGVtZW50KGVsZW1lbnQpKTtcbiAgfSBlbHNlIGlmIChpc0VsZW1lbnQoY2xpcHBpbmdBbmNlc3RvcikpIHtcbiAgICByZWN0ID0gZ2V0SW5uZXJCb3VuZGluZ0NsaWVudFJlY3QoY2xpcHBpbmdBbmNlc3Rvciwgc3RyYXRlZ3kpO1xuICB9IGVsc2Uge1xuICAgIGNvbnN0IHZpc3VhbE9mZnNldHMgPSBnZXRWaXN1YWxPZmZzZXRzKGVsZW1lbnQpO1xuICAgIHJlY3QgPSB7XG4gICAgICB4OiBjbGlwcGluZ0FuY2VzdG9yLnggLSB2aXN1YWxPZmZzZXRzLngsXG4gICAgICB5OiBjbGlwcGluZ0FuY2VzdG9yLnkgLSB2aXN1YWxPZmZzZXRzLnksXG4gICAgICB3aWR0aDogY2xpcHBpbmdBbmNlc3Rvci53aWR0aCxcbiAgICAgIGhlaWdodDogY2xpcHBpbmdBbmNlc3Rvci5oZWlnaHRcbiAgICB9O1xuICB9XG4gIHJldHVybiByZWN0VG9DbGllbnRSZWN0KHJlY3QpO1xufVxuZnVuY3Rpb24gaGFzRml4ZWRQb3NpdGlvbkFuY2VzdG9yKGVsZW1lbnQsIHN0b3BOb2RlKSB7XG4gIGNvbnN0IHBhcmVudE5vZGUgPSBnZXRQYXJlbnROb2RlKGVsZW1lbnQpO1xuICBpZiAocGFyZW50Tm9kZSA9PT0gc3RvcE5vZGUgfHwgIWlzRWxlbWVudChwYXJlbnROb2RlKSB8fCBpc0xhc3RUcmF2ZXJzYWJsZU5vZGUocGFyZW50Tm9kZSkpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cbiAgcmV0dXJuIGdldENvbXB1dGVkU3R5bGUocGFyZW50Tm9kZSkucG9zaXRpb24gPT09ICdmaXhlZCcgfHwgaGFzRml4ZWRQb3NpdGlvbkFuY2VzdG9yKHBhcmVudE5vZGUsIHN0b3BOb2RlKTtcbn1cblxuLy8gQSBcImNsaXBwaW5nIGFuY2VzdG9yXCIgaXMgYW4gYG92ZXJmbG93YCBlbGVtZW50IHdpdGggdGhlIGNoYXJhY3RlcmlzdGljIG9mXG4vLyBjbGlwcGluZyAob3IgaGlkaW5nKSBjaGlsZCBlbGVtZW50cy4gVGhpcyByZXR1cm5zIGFsbCBjbGlwcGluZyBhbmNlc3RvcnNcbi8vIG9mIHRoZSBnaXZlbiBlbGVtZW50IHVwIHRoZSB0cmVlLlxuZnVuY3Rpb24gZ2V0Q2xpcHBpbmdFbGVtZW50QW5jZXN0b3JzKGVsZW1lbnQsIGNhY2hlKSB7XG4gIGNvbnN0IGNhY2hlZFJlc3VsdCA9IGNhY2hlLmdldChlbGVtZW50KTtcbiAgaWYgKGNhY2hlZFJlc3VsdCkge1xuICAgIHJldHVybiBjYWNoZWRSZXN1bHQ7XG4gIH1cbiAgbGV0IHJlc3VsdCA9IGdldE92ZXJmbG93QW5jZXN0b3JzKGVsZW1lbnQsIFtdLCBmYWxzZSkuZmlsdGVyKGVsID0+IGlzRWxlbWVudChlbCkgJiYgZ2V0Tm9kZU5hbWUoZWwpICE9PSAnYm9keScpO1xuICBsZXQgY3VycmVudENvbnRhaW5pbmdCbG9ja0NvbXB1dGVkU3R5bGUgPSBudWxsO1xuICBjb25zdCBlbGVtZW50SXNGaXhlZCA9IGdldENvbXB1dGVkU3R5bGUoZWxlbWVudCkucG9zaXRpb24gPT09ICdmaXhlZCc7XG4gIGxldCBjdXJyZW50Tm9kZSA9IGVsZW1lbnRJc0ZpeGVkID8gZ2V0UGFyZW50Tm9kZShlbGVtZW50KSA6IGVsZW1lbnQ7XG5cbiAgLy8gaHR0cHM6Ly9kZXZlbG9wZXIubW96aWxsYS5vcmcvZW4tVVMvZG9jcy9XZWIvQ1NTL0NvbnRhaW5pbmdfYmxvY2sjaWRlbnRpZnlpbmdfdGhlX2NvbnRhaW5pbmdfYmxvY2tcbiAgd2hpbGUgKGlzRWxlbWVudChjdXJyZW50Tm9kZSkgJiYgIWlzTGFzdFRyYXZlcnNhYmxlTm9kZShjdXJyZW50Tm9kZSkpIHtcbiAgICBjb25zdCBjb21wdXRlZFN0eWxlID0gZ2V0Q29tcHV0ZWRTdHlsZShjdXJyZW50Tm9kZSk7XG4gICAgY29uc3QgY3VycmVudE5vZGVJc0NvbnRhaW5pbmcgPSBpc0NvbnRhaW5pbmdCbG9jayhjdXJyZW50Tm9kZSk7XG4gICAgaWYgKCFjdXJyZW50Tm9kZUlzQ29udGFpbmluZyAmJiBjb21wdXRlZFN0eWxlLnBvc2l0aW9uID09PSAnZml4ZWQnKSB7XG4gICAgICBjdXJyZW50Q29udGFpbmluZ0Jsb2NrQ29tcHV0ZWRTdHlsZSA9IG51bGw7XG4gICAgfVxuICAgIGNvbnN0IHNob3VsZERyb3BDdXJyZW50Tm9kZSA9IGVsZW1lbnRJc0ZpeGVkID8gIWN1cnJlbnROb2RlSXNDb250YWluaW5nICYmICFjdXJyZW50Q29udGFpbmluZ0Jsb2NrQ29tcHV0ZWRTdHlsZSA6ICFjdXJyZW50Tm9kZUlzQ29udGFpbmluZyAmJiBjb21wdXRlZFN0eWxlLnBvc2l0aW9uID09PSAnc3RhdGljJyAmJiAhIWN1cnJlbnRDb250YWluaW5nQmxvY2tDb21wdXRlZFN0eWxlICYmIFsnYWJzb2x1dGUnLCAnZml4ZWQnXS5pbmNsdWRlcyhjdXJyZW50Q29udGFpbmluZ0Jsb2NrQ29tcHV0ZWRTdHlsZS5wb3NpdGlvbikgfHwgaXNPdmVyZmxvd0VsZW1lbnQoY3VycmVudE5vZGUpICYmICFjdXJyZW50Tm9kZUlzQ29udGFpbmluZyAmJiBoYXNGaXhlZFBvc2l0aW9uQW5jZXN0b3IoZWxlbWVudCwgY3VycmVudE5vZGUpO1xuICAgIGlmIChzaG91bGREcm9wQ3VycmVudE5vZGUpIHtcbiAgICAgIC8vIERyb3Agbm9uLWNvbnRhaW5pbmcgYmxvY2tzLlxuICAgICAgcmVzdWx0ID0gcmVzdWx0LmZpbHRlcihhbmNlc3RvciA9PiBhbmNlc3RvciAhPT0gY3VycmVudE5vZGUpO1xuICAgIH0gZWxzZSB7XG4gICAgICAvLyBSZWNvcmQgbGFzdCBjb250YWluaW5nIGJsb2NrIGZvciBuZXh0IGl0ZXJhdGlvbi5cbiAgICAgIGN1cnJlbnRDb250YWluaW5nQmxvY2tDb21wdXRlZFN0eWxlID0gY29tcHV0ZWRTdHlsZTtcbiAgICB9XG4gICAgY3VycmVudE5vZGUgPSBnZXRQYXJlbnROb2RlKGN1cnJlbnROb2RlKTtcbiAgfVxuICBjYWNoZS5zZXQoZWxlbWVudCwgcmVzdWx0KTtcbiAgcmV0dXJuIHJlc3VsdDtcbn1cblxuLy8gR2V0cyB0aGUgbWF4aW11bSBhcmVhIHRoYXQgdGhlIGVsZW1lbnQgaXMgdmlzaWJsZSBpbiBkdWUgdG8gYW55IG51bWJlciBvZlxuLy8gY2xpcHBpbmcgYW5jZXN0b3JzLlxuZnVuY3Rpb24gZ2V0Q2xpcHBpbmdSZWN0KF9yZWYpIHtcbiAgbGV0IHtcbiAgICBlbGVtZW50LFxuICAgIGJvdW5kYXJ5LFxuICAgIHJvb3RCb3VuZGFyeSxcbiAgICBzdHJhdGVneVxuICB9ID0gX3JlZjtcbiAgY29uc3QgZWxlbWVudENsaXBwaW5nQW5jZXN0b3JzID0gYm91bmRhcnkgPT09ICdjbGlwcGluZ0FuY2VzdG9ycycgPyBpc1RvcExheWVyKGVsZW1lbnQpID8gW10gOiBnZXRDbGlwcGluZ0VsZW1lbnRBbmNlc3RvcnMoZWxlbWVudCwgdGhpcy5fYykgOiBbXS5jb25jYXQoYm91bmRhcnkpO1xuICBjb25zdCBjbGlwcGluZ0FuY2VzdG9ycyA9IFsuLi5lbGVtZW50Q2xpcHBpbmdBbmNlc3RvcnMsIHJvb3RCb3VuZGFyeV07XG4gIGNvbnN0IGZpcnN0Q2xpcHBpbmdBbmNlc3RvciA9IGNsaXBwaW5nQW5jZXN0b3JzWzBdO1xuICBjb25zdCBjbGlwcGluZ1JlY3QgPSBjbGlwcGluZ0FuY2VzdG9ycy5yZWR1Y2UoKGFjY1JlY3QsIGNsaXBwaW5nQW5jZXN0b3IpID0+IHtcbiAgICBjb25zdCByZWN0ID0gZ2V0Q2xpZW50UmVjdEZyb21DbGlwcGluZ0FuY2VzdG9yKGVsZW1lbnQsIGNsaXBwaW5nQW5jZXN0b3IsIHN0cmF0ZWd5KTtcbiAgICBhY2NSZWN0LnRvcCA9IG1heChyZWN0LnRvcCwgYWNjUmVjdC50b3ApO1xuICAgIGFjY1JlY3QucmlnaHQgPSBtaW4ocmVjdC5yaWdodCwgYWNjUmVjdC5yaWdodCk7XG4gICAgYWNjUmVjdC5ib3R0b20gPSBtaW4ocmVjdC5ib3R0b20sIGFjY1JlY3QuYm90dG9tKTtcbiAgICBhY2NSZWN0LmxlZnQgPSBtYXgocmVjdC5sZWZ0LCBhY2NSZWN0LmxlZnQpO1xuICAgIHJldHVybiBhY2NSZWN0O1xuICB9LCBnZXRDbGllbnRSZWN0RnJvbUNsaXBwaW5nQW5jZXN0b3IoZWxlbWVudCwgZmlyc3RDbGlwcGluZ0FuY2VzdG9yLCBzdHJhdGVneSkpO1xuICByZXR1cm4ge1xuICAgIHdpZHRoOiBjbGlwcGluZ1JlY3QucmlnaHQgLSBjbGlwcGluZ1JlY3QubGVmdCxcbiAgICBoZWlnaHQ6IGNsaXBwaW5nUmVjdC5ib3R0b20gLSBjbGlwcGluZ1JlY3QudG9wLFxuICAgIHg6IGNsaXBwaW5nUmVjdC5sZWZ0LFxuICAgIHk6IGNsaXBwaW5nUmVjdC50b3BcbiAgfTtcbn1cblxuZnVuY3Rpb24gZ2V0RGltZW5zaW9ucyhlbGVtZW50KSB7XG4gIGNvbnN0IHtcbiAgICB3aWR0aCxcbiAgICBoZWlnaHRcbiAgfSA9IGdldENzc0RpbWVuc2lvbnMoZWxlbWVudCk7XG4gIHJldHVybiB7XG4gICAgd2lkdGgsXG4gICAgaGVpZ2h0XG4gIH07XG59XG5cbmZ1bmN0aW9uIGdldFJlY3RSZWxhdGl2ZVRvT2Zmc2V0UGFyZW50KGVsZW1lbnQsIG9mZnNldFBhcmVudCwgc3RyYXRlZ3kpIHtcbiAgY29uc3QgaXNPZmZzZXRQYXJlbnRBbkVsZW1lbnQgPSBpc0hUTUxFbGVtZW50KG9mZnNldFBhcmVudCk7XG4gIGNvbnN0IGRvY3VtZW50RWxlbWVudCA9IGdldERvY3VtZW50RWxlbWVudChvZmZzZXRQYXJlbnQpO1xuICBjb25zdCBpc0ZpeGVkID0gc3RyYXRlZ3kgPT09ICdmaXhlZCc7XG4gIGNvbnN0IHJlY3QgPSBnZXRCb3VuZGluZ0NsaWVudFJlY3QoZWxlbWVudCwgdHJ1ZSwgaXNGaXhlZCwgb2Zmc2V0UGFyZW50KTtcbiAgbGV0IHNjcm9sbCA9IHtcbiAgICBzY3JvbGxMZWZ0OiAwLFxuICAgIHNjcm9sbFRvcDogMFxuICB9O1xuICBjb25zdCBvZmZzZXRzID0gY3JlYXRlQ29vcmRzKDApO1xuICBpZiAoaXNPZmZzZXRQYXJlbnRBbkVsZW1lbnQgfHwgIWlzT2Zmc2V0UGFyZW50QW5FbGVtZW50ICYmICFpc0ZpeGVkKSB7XG4gICAgaWYgKGdldE5vZGVOYW1lKG9mZnNldFBhcmVudCkgIT09ICdib2R5JyB8fCBpc092ZXJmbG93RWxlbWVudChkb2N1bWVudEVsZW1lbnQpKSB7XG4gICAgICBzY3JvbGwgPSBnZXROb2RlU2Nyb2xsKG9mZnNldFBhcmVudCk7XG4gICAgfVxuICAgIGlmIChpc09mZnNldFBhcmVudEFuRWxlbWVudCkge1xuICAgICAgY29uc3Qgb2Zmc2V0UmVjdCA9IGdldEJvdW5kaW5nQ2xpZW50UmVjdChvZmZzZXRQYXJlbnQsIHRydWUsIGlzRml4ZWQsIG9mZnNldFBhcmVudCk7XG4gICAgICBvZmZzZXRzLnggPSBvZmZzZXRSZWN0LnggKyBvZmZzZXRQYXJlbnQuY2xpZW50TGVmdDtcbiAgICAgIG9mZnNldHMueSA9IG9mZnNldFJlY3QueSArIG9mZnNldFBhcmVudC5jbGllbnRUb3A7XG4gICAgfSBlbHNlIGlmIChkb2N1bWVudEVsZW1lbnQpIHtcbiAgICAgIC8vIElmIHRoZSA8Ym9keT4gc2Nyb2xsYmFyIGFwcGVhcnMgb24gdGhlIGxlZnQgKGUuZy4gUlRMIHN5c3RlbXMpLiBVc2VcbiAgICAgIC8vIEZpcmVmb3ggd2l0aCBsYXlvdXQuc2Nyb2xsYmFyLnNpZGUgPSAzIGluIGFib3V0OmNvbmZpZyB0byB0ZXN0IHRoaXMuXG4gICAgICBvZmZzZXRzLnggPSBnZXRXaW5kb3dTY3JvbGxCYXJYKGRvY3VtZW50RWxlbWVudCk7XG4gICAgfVxuICB9XG4gIGNvbnN0IGh0bWxPZmZzZXQgPSBkb2N1bWVudEVsZW1lbnQgJiYgIWlzT2Zmc2V0UGFyZW50QW5FbGVtZW50ICYmICFpc0ZpeGVkID8gZ2V0SFRNTE9mZnNldChkb2N1bWVudEVsZW1lbnQsIHNjcm9sbCkgOiBjcmVhdGVDb29yZHMoMCk7XG4gIGNvbnN0IHggPSByZWN0LmxlZnQgKyBzY3JvbGwuc2Nyb2xsTGVmdCAtIG9mZnNldHMueCAtIGh0bWxPZmZzZXQueDtcbiAgY29uc3QgeSA9IHJlY3QudG9wICsgc2Nyb2xsLnNjcm9sbFRvcCAtIG9mZnNldHMueSAtIGh0bWxPZmZzZXQueTtcbiAgcmV0dXJuIHtcbiAgICB4LFxuICAgIHksXG4gICAgd2lkdGg6IHJlY3Qud2lkdGgsXG4gICAgaGVpZ2h0OiByZWN0LmhlaWdodFxuICB9O1xufVxuXG5mdW5jdGlvbiBpc1N0YXRpY1Bvc2l0aW9uZWQoZWxlbWVudCkge1xuICByZXR1cm4gZ2V0Q29tcHV0ZWRTdHlsZShlbGVtZW50KS5wb3NpdGlvbiA9PT0gJ3N0YXRpYyc7XG59XG5cbmZ1bmN0aW9uIGdldFRydWVPZmZzZXRQYXJlbnQoZWxlbWVudCwgcG9seWZpbGwpIHtcbiAgaWYgKCFpc0hUTUxFbGVtZW50KGVsZW1lbnQpIHx8IGdldENvbXB1dGVkU3R5bGUoZWxlbWVudCkucG9zaXRpb24gPT09ICdmaXhlZCcpIHtcbiAgICByZXR1cm4gbnVsbDtcbiAgfVxuICBpZiAocG9seWZpbGwpIHtcbiAgICByZXR1cm4gcG9seWZpbGwoZWxlbWVudCk7XG4gIH1cbiAgbGV0IHJhd09mZnNldFBhcmVudCA9IGVsZW1lbnQub2Zmc2V0UGFyZW50O1xuXG4gIC8vIEZpcmVmb3ggcmV0dXJucyB0aGUgPGh0bWw+IGVsZW1lbnQgYXMgdGhlIG9mZnNldFBhcmVudCBpZiBpdCdzIG5vbi1zdGF0aWMsXG4gIC8vIHdoaWxlIENocm9tZSBhbmQgU2FmYXJpIHJldHVybiB0aGUgPGJvZHk+IGVsZW1lbnQuIFRoZSA8Ym9keT4gZWxlbWVudCBtdXN0XG4gIC8vIGJlIHVzZWQgdG8gcGVyZm9ybSB0aGUgY29ycmVjdCBjYWxjdWxhdGlvbnMgZXZlbiBpZiB0aGUgPGh0bWw+IGVsZW1lbnQgaXNcbiAgLy8gbm9uLXN0YXRpYy5cbiAgaWYgKGdldERvY3VtZW50RWxlbWVudChlbGVtZW50KSA9PT0gcmF3T2Zmc2V0UGFyZW50KSB7XG4gICAgcmF3T2Zmc2V0UGFyZW50ID0gcmF3T2Zmc2V0UGFyZW50Lm93bmVyRG9jdW1lbnQuYm9keTtcbiAgfVxuICByZXR1cm4gcmF3T2Zmc2V0UGFyZW50O1xufVxuXG4vLyBHZXRzIHRoZSBjbG9zZXN0IGFuY2VzdG9yIHBvc2l0aW9uZWQgZWxlbWVudC4gSGFuZGxlcyBzb21lIGVkZ2UgY2FzZXMsXG4vLyBzdWNoIGFzIHRhYmxlIGFuY2VzdG9ycyBhbmQgY3Jvc3MgYnJvd3NlciBidWdzLlxuZnVuY3Rpb24gZ2V0T2Zmc2V0UGFyZW50KGVsZW1lbnQsIHBvbHlmaWxsKSB7XG4gIGNvbnN0IHdpbiA9IGdldFdpbmRvdyhlbGVtZW50KTtcbiAgaWYgKGlzVG9wTGF5ZXIoZWxlbWVudCkpIHtcbiAgICByZXR1cm4gd2luO1xuICB9XG4gIGlmICghaXNIVE1MRWxlbWVudChlbGVtZW50KSkge1xuICAgIGxldCBzdmdPZmZzZXRQYXJlbnQgPSBnZXRQYXJlbnROb2RlKGVsZW1lbnQpO1xuICAgIHdoaWxlIChzdmdPZmZzZXRQYXJlbnQgJiYgIWlzTGFzdFRyYXZlcnNhYmxlTm9kZShzdmdPZmZzZXRQYXJlbnQpKSB7XG4gICAgICBpZiAoaXNFbGVtZW50KHN2Z09mZnNldFBhcmVudCkgJiYgIWlzU3RhdGljUG9zaXRpb25lZChzdmdPZmZzZXRQYXJlbnQpKSB7XG4gICAgICAgIHJldHVybiBzdmdPZmZzZXRQYXJlbnQ7XG4gICAgICB9XG4gICAgICBzdmdPZmZzZXRQYXJlbnQgPSBnZXRQYXJlbnROb2RlKHN2Z09mZnNldFBhcmVudCk7XG4gICAgfVxuICAgIHJldHVybiB3aW47XG4gIH1cbiAgbGV0IG9mZnNldFBhcmVudCA9IGdldFRydWVPZmZzZXRQYXJlbnQoZWxlbWVudCwgcG9seWZpbGwpO1xuICB3aGlsZSAob2Zmc2V0UGFyZW50ICYmIGlzVGFibGVFbGVtZW50KG9mZnNldFBhcmVudCkgJiYgaXNTdGF0aWNQb3NpdGlvbmVkKG9mZnNldFBhcmVudCkpIHtcbiAgICBvZmZzZXRQYXJlbnQgPSBnZXRUcnVlT2Zmc2V0UGFyZW50KG9mZnNldFBhcmVudCwgcG9seWZpbGwpO1xuICB9XG4gIGlmIChvZmZzZXRQYXJlbnQgJiYgaXNMYXN0VHJhdmVyc2FibGVOb2RlKG9mZnNldFBhcmVudCkgJiYgaXNTdGF0aWNQb3NpdGlvbmVkKG9mZnNldFBhcmVudCkgJiYgIWlzQ29udGFpbmluZ0Jsb2NrKG9mZnNldFBhcmVudCkpIHtcbiAgICByZXR1cm4gd2luO1xuICB9XG4gIHJldHVybiBvZmZzZXRQYXJlbnQgfHwgZ2V0Q29udGFpbmluZ0Jsb2NrKGVsZW1lbnQpIHx8IHdpbjtcbn1cblxuY29uc3QgZ2V0RWxlbWVudFJlY3RzID0gYXN5bmMgZnVuY3Rpb24gKGRhdGEpIHtcbiAgY29uc3QgZ2V0T2Zmc2V0UGFyZW50Rm4gPSB0aGlzLmdldE9mZnNldFBhcmVudCB8fCBnZXRPZmZzZXRQYXJlbnQ7XG4gIGNvbnN0IGdldERpbWVuc2lvbnNGbiA9IHRoaXMuZ2V0RGltZW5zaW9ucztcbiAgY29uc3QgZmxvYXRpbmdEaW1lbnNpb25zID0gYXdhaXQgZ2V0RGltZW5zaW9uc0ZuKGRhdGEuZmxvYXRpbmcpO1xuICByZXR1cm4ge1xuICAgIHJlZmVyZW5jZTogZ2V0UmVjdFJlbGF0aXZlVG9PZmZzZXRQYXJlbnQoZGF0YS5yZWZlcmVuY2UsIGF3YWl0IGdldE9mZnNldFBhcmVudEZuKGRhdGEuZmxvYXRpbmcpLCBkYXRhLnN0cmF0ZWd5KSxcbiAgICBmbG9hdGluZzoge1xuICAgICAgeDogMCxcbiAgICAgIHk6IDAsXG4gICAgICB3aWR0aDogZmxvYXRpbmdEaW1lbnNpb25zLndpZHRoLFxuICAgICAgaGVpZ2h0OiBmbG9hdGluZ0RpbWVuc2lvbnMuaGVpZ2h0XG4gICAgfVxuICB9O1xufTtcblxuZnVuY3Rpb24gaXNSVEwoZWxlbWVudCkge1xuICByZXR1cm4gZ2V0Q29tcHV0ZWRTdHlsZShlbGVtZW50KS5kaXJlY3Rpb24gPT09ICdydGwnO1xufVxuXG5jb25zdCBwbGF0Zm9ybSA9IHtcbiAgY29udmVydE9mZnNldFBhcmVudFJlbGF0aXZlUmVjdFRvVmlld3BvcnRSZWxhdGl2ZVJlY3QsXG4gIGdldERvY3VtZW50RWxlbWVudCxcbiAgZ2V0Q2xpcHBpbmdSZWN0LFxuICBnZXRPZmZzZXRQYXJlbnQsXG4gIGdldEVsZW1lbnRSZWN0cyxcbiAgZ2V0Q2xpZW50UmVjdHMsXG4gIGdldERpbWVuc2lvbnMsXG4gIGdldFNjYWxlLFxuICBpc0VsZW1lbnQsXG4gIGlzUlRMXG59O1xuXG4vLyBodHRwczovL3NhbXRob3IuYXUvMjAyMS9vYnNlcnZpbmctZG9tL1xuZnVuY3Rpb24gb2JzZXJ2ZU1vdmUoZWxlbWVudCwgb25Nb3ZlKSB7XG4gIGxldCBpbyA9IG51bGw7XG4gIGxldCB0aW1lb3V0SWQ7XG4gIGNvbnN0IHJvb3QgPSBnZXREb2N1bWVudEVsZW1lbnQoZWxlbWVudCk7XG4gIGZ1bmN0aW9uIGNsZWFudXAoKSB7XG4gICAgdmFyIF9pbztcbiAgICBjbGVhclRpbWVvdXQodGltZW91dElkKTtcbiAgICAoX2lvID0gaW8pID09IG51bGwgfHwgX2lvLmRpc2Nvbm5lY3QoKTtcbiAgICBpbyA9IG51bGw7XG4gIH1cbiAgZnVuY3Rpb24gcmVmcmVzaChza2lwLCB0aHJlc2hvbGQpIHtcbiAgICBpZiAoc2tpcCA9PT0gdm9pZCAwKSB7XG4gICAgICBza2lwID0gZmFsc2U7XG4gICAgfVxuICAgIGlmICh0aHJlc2hvbGQgPT09IHZvaWQgMCkge1xuICAgICAgdGhyZXNob2xkID0gMTtcbiAgICB9XG4gICAgY2xlYW51cCgpO1xuICAgIGNvbnN0IHtcbiAgICAgIGxlZnQsXG4gICAgICB0b3AsXG4gICAgICB3aWR0aCxcbiAgICAgIGhlaWdodFxuICAgIH0gPSBlbGVtZW50LmdldEJvdW5kaW5nQ2xpZW50UmVjdCgpO1xuICAgIGlmICghc2tpcCkge1xuICAgICAgb25Nb3ZlKCk7XG4gICAgfVxuICAgIGlmICghd2lkdGggfHwgIWhlaWdodCkge1xuICAgICAgcmV0dXJuO1xuICAgIH1cbiAgICBjb25zdCBpbnNldFRvcCA9IGZsb29yKHRvcCk7XG4gICAgY29uc3QgaW5zZXRSaWdodCA9IGZsb29yKHJvb3QuY2xpZW50V2lkdGggLSAobGVmdCArIHdpZHRoKSk7XG4gICAgY29uc3QgaW5zZXRCb3R0b20gPSBmbG9vcihyb290LmNsaWVudEhlaWdodCAtICh0b3AgKyBoZWlnaHQpKTtcbiAgICBjb25zdCBpbnNldExlZnQgPSBmbG9vcihsZWZ0KTtcbiAgICBjb25zdCByb290TWFyZ2luID0gLWluc2V0VG9wICsgXCJweCBcIiArIC1pbnNldFJpZ2h0ICsgXCJweCBcIiArIC1pbnNldEJvdHRvbSArIFwicHggXCIgKyAtaW5zZXRMZWZ0ICsgXCJweFwiO1xuICAgIGNvbnN0IG9wdGlvbnMgPSB7XG4gICAgICByb290TWFyZ2luLFxuICAgICAgdGhyZXNob2xkOiBtYXgoMCwgbWluKDEsIHRocmVzaG9sZCkpIHx8IDFcbiAgICB9O1xuICAgIGxldCBpc0ZpcnN0VXBkYXRlID0gdHJ1ZTtcbiAgICBmdW5jdGlvbiBoYW5kbGVPYnNlcnZlKGVudHJpZXMpIHtcbiAgICAgIGNvbnN0IHJhdGlvID0gZW50cmllc1swXS5pbnRlcnNlY3Rpb25SYXRpbztcbiAgICAgIGlmIChyYXRpbyAhPT0gdGhyZXNob2xkKSB7XG4gICAgICAgIGlmICghaXNGaXJzdFVwZGF0ZSkge1xuICAgICAgICAgIHJldHVybiByZWZyZXNoKCk7XG4gICAgICAgIH1cbiAgICAgICAgaWYgKCFyYXRpbykge1xuICAgICAgICAgIC8vIElmIHRoZSByZWZlcmVuY2UgaXMgY2xpcHBlZCwgdGhlIHJhdGlvIGlzIDAuIFRocm90dGxlIHRoZSByZWZyZXNoXG4gICAgICAgICAgLy8gdG8gcHJldmVudCBhbiBpbmZpbml0ZSBsb29wIG9mIHVwZGF0ZXMuXG4gICAgICAgICAgdGltZW91dElkID0gc2V0VGltZW91dCgoKSA9PiB7XG4gICAgICAgICAgICByZWZyZXNoKGZhbHNlLCAxZS03KTtcbiAgICAgICAgICB9LCAxMDAwKTtcbiAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICByZWZyZXNoKGZhbHNlLCByYXRpbyk7XG4gICAgICAgIH1cbiAgICAgIH1cbiAgICAgIGlzRmlyc3RVcGRhdGUgPSBmYWxzZTtcbiAgICB9XG5cbiAgICAvLyBPbGRlciBicm93c2VycyBkb24ndCBzdXBwb3J0IGEgYGRvY3VtZW50YCBhcyB0aGUgcm9vdCBhbmQgd2lsbCB0aHJvdyBhblxuICAgIC8vIGVycm9yLlxuICAgIHRyeSB7XG4gICAgICBpbyA9IG5ldyBJbnRlcnNlY3Rpb25PYnNlcnZlcihoYW5kbGVPYnNlcnZlLCB7XG4gICAgICAgIC4uLm9wdGlvbnMsXG4gICAgICAgIC8vIEhhbmRsZSA8aWZyYW1lPnNcbiAgICAgICAgcm9vdDogcm9vdC5vd25lckRvY3VtZW50XG4gICAgICB9KTtcbiAgICB9IGNhdGNoIChlKSB7XG4gICAgICBpbyA9IG5ldyBJbnRlcnNlY3Rpb25PYnNlcnZlcihoYW5kbGVPYnNlcnZlLCBvcHRpb25zKTtcbiAgICB9XG4gICAgaW8ub2JzZXJ2ZShlbGVtZW50KTtcbiAgfVxuICByZWZyZXNoKHRydWUpO1xuICByZXR1cm4gY2xlYW51cDtcbn1cblxuLyoqXG4gKiBBdXRvbWF0aWNhbGx5IHVwZGF0ZXMgdGhlIHBvc2l0aW9uIG9mIHRoZSBmbG9hdGluZyBlbGVtZW50IHdoZW4gbmVjZXNzYXJ5LlxuICogU2hvdWxkIG9ubHkgYmUgY2FsbGVkIHdoZW4gdGhlIGZsb2F0aW5nIGVsZW1lbnQgaXMgbW91bnRlZCBvbiB0aGUgRE9NIG9yXG4gKiB2aXNpYmxlIG9uIHRoZSBzY3JlZW4uXG4gKiBAcmV0dXJucyBjbGVhbnVwIGZ1bmN0aW9uIHRoYXQgc2hvdWxkIGJlIGludm9rZWQgd2hlbiB0aGUgZmxvYXRpbmcgZWxlbWVudCBpc1xuICogcmVtb3ZlZCBmcm9tIHRoZSBET00gb3IgaGlkZGVuIGZyb20gdGhlIHNjcmVlbi5cbiAqIEBzZWUgaHR0cHM6Ly9mbG9hdGluZy11aS5jb20vZG9jcy9hdXRvVXBkYXRlXG4gKi9cbmZ1bmN0aW9uIGF1dG9VcGRhdGUocmVmZXJlbmNlLCBmbG9hdGluZywgdXBkYXRlLCBvcHRpb25zKSB7XG4gIGlmIChvcHRpb25zID09PSB2b2lkIDApIHtcbiAgICBvcHRpb25zID0ge307XG4gIH1cbiAgY29uc3Qge1xuICAgIGFuY2VzdG9yU2Nyb2xsID0gdHJ1ZSxcbiAgICBhbmNlc3RvclJlc2l6ZSA9IHRydWUsXG4gICAgZWxlbWVudFJlc2l6ZSA9IHR5cGVvZiBSZXNpemVPYnNlcnZlciA9PT0gJ2Z1bmN0aW9uJyxcbiAgICBsYXlvdXRTaGlmdCA9IHR5cGVvZiBJbnRlcnNlY3Rpb25PYnNlcnZlciA9PT0gJ2Z1bmN0aW9uJyxcbiAgICBhbmltYXRpb25GcmFtZSA9IGZhbHNlXG4gIH0gPSBvcHRpb25zO1xuICBjb25zdCByZWZlcmVuY2VFbCA9IHVud3JhcEVsZW1lbnQocmVmZXJlbmNlKTtcbiAgY29uc3QgYW5jZXN0b3JzID0gYW5jZXN0b3JTY3JvbGwgfHwgYW5jZXN0b3JSZXNpemUgPyBbLi4uKHJlZmVyZW5jZUVsID8gZ2V0T3ZlcmZsb3dBbmNlc3RvcnMocmVmZXJlbmNlRWwpIDogW10pLCAuLi5nZXRPdmVyZmxvd0FuY2VzdG9ycyhmbG9hdGluZyldIDogW107XG4gIGFuY2VzdG9ycy5mb3JFYWNoKGFuY2VzdG9yID0+IHtcbiAgICBhbmNlc3RvclNjcm9sbCAmJiBhbmNlc3Rvci5hZGRFdmVudExpc3RlbmVyKCdzY3JvbGwnLCB1cGRhdGUsIHtcbiAgICAgIHBhc3NpdmU6IHRydWVcbiAgICB9KTtcbiAgICBhbmNlc3RvclJlc2l6ZSAmJiBhbmNlc3Rvci5hZGRFdmVudExpc3RlbmVyKCdyZXNpemUnLCB1cGRhdGUpO1xuICB9KTtcbiAgY29uc3QgY2xlYW51cElvID0gcmVmZXJlbmNlRWwgJiYgbGF5b3V0U2hpZnQgPyBvYnNlcnZlTW92ZShyZWZlcmVuY2VFbCwgdXBkYXRlKSA6IG51bGw7XG4gIGxldCByZW9ic2VydmVGcmFtZSA9IC0xO1xuICBsZXQgcmVzaXplT2JzZXJ2ZXIgPSBudWxsO1xuICBpZiAoZWxlbWVudFJlc2l6ZSkge1xuICAgIHJlc2l6ZU9ic2VydmVyID0gbmV3IFJlc2l6ZU9ic2VydmVyKF9yZWYgPT4ge1xuICAgICAgbGV0IFtmaXJzdEVudHJ5XSA9IF9yZWY7XG4gICAgICBpZiAoZmlyc3RFbnRyeSAmJiBmaXJzdEVudHJ5LnRhcmdldCA9PT0gcmVmZXJlbmNlRWwgJiYgcmVzaXplT2JzZXJ2ZXIpIHtcbiAgICAgICAgLy8gUHJldmVudCB1cGRhdGUgbG9vcHMgd2hlbiB1c2luZyB0aGUgYHNpemVgIG1pZGRsZXdhcmUuXG4gICAgICAgIC8vIGh0dHBzOi8vZ2l0aHViLmNvbS9mbG9hdGluZy11aS9mbG9hdGluZy11aS9pc3N1ZXMvMTc0MFxuICAgICAgICByZXNpemVPYnNlcnZlci51bm9ic2VydmUoZmxvYXRpbmcpO1xuICAgICAgICBjYW5jZWxBbmltYXRpb25GcmFtZShyZW9ic2VydmVGcmFtZSk7XG4gICAgICAgIHJlb2JzZXJ2ZUZyYW1lID0gcmVxdWVzdEFuaW1hdGlvbkZyYW1lKCgpID0+IHtcbiAgICAgICAgICB2YXIgX3Jlc2l6ZU9ic2VydmVyO1xuICAgICAgICAgIChfcmVzaXplT2JzZXJ2ZXIgPSByZXNpemVPYnNlcnZlcikgPT0gbnVsbCB8fCBfcmVzaXplT2JzZXJ2ZXIub2JzZXJ2ZShmbG9hdGluZyk7XG4gICAgICAgIH0pO1xuICAgICAgfVxuICAgICAgdXBkYXRlKCk7XG4gICAgfSk7XG4gICAgaWYgKHJlZmVyZW5jZUVsICYmICFhbmltYXRpb25GcmFtZSkge1xuICAgICAgcmVzaXplT2JzZXJ2ZXIub2JzZXJ2ZShyZWZlcmVuY2VFbCk7XG4gICAgfVxuICAgIHJlc2l6ZU9ic2VydmVyLm9ic2VydmUoZmxvYXRpbmcpO1xuICB9XG4gIGxldCBmcmFtZUlkO1xuICBsZXQgcHJldlJlZlJlY3QgPSBhbmltYXRpb25GcmFtZSA/IGdldEJvdW5kaW5nQ2xpZW50UmVjdChyZWZlcmVuY2UpIDogbnVsbDtcbiAgaWYgKGFuaW1hdGlvbkZyYW1lKSB7XG4gICAgZnJhbWVMb29wKCk7XG4gIH1cbiAgZnVuY3Rpb24gZnJhbWVMb29wKCkge1xuICAgIGNvbnN0IG5leHRSZWZSZWN0ID0gZ2V0Qm91bmRpbmdDbGllbnRSZWN0KHJlZmVyZW5jZSk7XG4gICAgaWYgKHByZXZSZWZSZWN0ICYmIChuZXh0UmVmUmVjdC54ICE9PSBwcmV2UmVmUmVjdC54IHx8IG5leHRSZWZSZWN0LnkgIT09IHByZXZSZWZSZWN0LnkgfHwgbmV4dFJlZlJlY3Qud2lkdGggIT09IHByZXZSZWZSZWN0LndpZHRoIHx8IG5leHRSZWZSZWN0LmhlaWdodCAhPT0gcHJldlJlZlJlY3QuaGVpZ2h0KSkge1xuICAgICAgdXBkYXRlKCk7XG4gICAgfVxuICAgIHByZXZSZWZSZWN0ID0gbmV4dFJlZlJlY3Q7XG4gICAgZnJhbWVJZCA9IHJlcXVlc3RBbmltYXRpb25GcmFtZShmcmFtZUxvb3ApO1xuICB9XG4gIHVwZGF0ZSgpO1xuICByZXR1cm4gKCkgPT4ge1xuICAgIHZhciBfcmVzaXplT2JzZXJ2ZXIyO1xuICAgIGFuY2VzdG9ycy5mb3JFYWNoKGFuY2VzdG9yID0+IHtcbiAgICAgIGFuY2VzdG9yU2Nyb2xsICYmIGFuY2VzdG9yLnJlbW92ZUV2ZW50TGlzdGVuZXIoJ3Njcm9sbCcsIHVwZGF0ZSk7XG4gICAgICBhbmNlc3RvclJlc2l6ZSAmJiBhbmNlc3Rvci5yZW1vdmVFdmVudExpc3RlbmVyKCdyZXNpemUnLCB1cGRhdGUpO1xuICAgIH0pO1xuICAgIGNsZWFudXBJbyA9PSBudWxsIHx8IGNsZWFudXBJbygpO1xuICAgIChfcmVzaXplT2JzZXJ2ZXIyID0gcmVzaXplT2JzZXJ2ZXIpID09IG51bGwgfHwgX3Jlc2l6ZU9ic2VydmVyMi5kaXNjb25uZWN0KCk7XG4gICAgcmVzaXplT2JzZXJ2ZXIgPSBudWxsO1xuICAgIGlmIChhbmltYXRpb25GcmFtZSkge1xuICAgICAgY2FuY2VsQW5pbWF0aW9uRnJhbWUoZnJhbWVJZCk7XG4gICAgfVxuICB9O1xufVxuXG4vKipcbiAqIFJlc29sdmVzIHdpdGggYW4gb2JqZWN0IG9mIG92ZXJmbG93IHNpZGUgb2Zmc2V0cyB0aGF0IGRldGVybWluZSBob3cgbXVjaCB0aGVcbiAqIGVsZW1lbnQgaXMgb3ZlcmZsb3dpbmcgYSBnaXZlbiBjbGlwcGluZyBib3VuZGFyeSBvbiBlYWNoIHNpZGUuXG4gKiAtIHBvc2l0aXZlID0gb3ZlcmZsb3dpbmcgdGhlIGJvdW5kYXJ5IGJ5IHRoYXQgbnVtYmVyIG9mIHBpeGVsc1xuICogLSBuZWdhdGl2ZSA9IGhvdyBtYW55IHBpeGVscyBsZWZ0IGJlZm9yZSBpdCB3aWxsIG92ZXJmbG93XG4gKiAtIDAgPSBsaWVzIGZsdXNoIHdpdGggdGhlIGJvdW5kYXJ5XG4gKiBAc2VlIGh0dHBzOi8vZmxvYXRpbmctdWkuY29tL2RvY3MvZGV0ZWN0T3ZlcmZsb3dcbiAqL1xuY29uc3QgZGV0ZWN0T3ZlcmZsb3cgPSBkZXRlY3RPdmVyZmxvdyQxO1xuXG4vKipcbiAqIE1vZGlmaWVzIHRoZSBwbGFjZW1lbnQgYnkgdHJhbnNsYXRpbmcgdGhlIGZsb2F0aW5nIGVsZW1lbnQgYWxvbmcgdGhlXG4gKiBzcGVjaWZpZWQgYXhlcy5cbiAqIEEgbnVtYmVyIChzaG9ydGhhbmQgZm9yIGBtYWluQXhpc2Agb3IgZGlzdGFuY2UpLCBvciBhbiBheGVzIGNvbmZpZ3VyYXRpb25cbiAqIG9iamVjdCBtYXkgYmUgcGFzc2VkLlxuICogQHNlZSBodHRwczovL2Zsb2F0aW5nLXVpLmNvbS9kb2NzL29mZnNldFxuICovXG5jb25zdCBvZmZzZXQgPSBvZmZzZXQkMTtcblxuLyoqXG4gKiBPcHRpbWl6ZXMgdGhlIHZpc2liaWxpdHkgb2YgdGhlIGZsb2F0aW5nIGVsZW1lbnQgYnkgY2hvb3NpbmcgdGhlIHBsYWNlbWVudFxuICogdGhhdCBoYXMgdGhlIG1vc3Qgc3BhY2UgYXZhaWxhYmxlIGF1dG9tYXRpY2FsbHksIHdpdGhvdXQgbmVlZGluZyB0byBzcGVjaWZ5IGFcbiAqIHByZWZlcnJlZCBwbGFjZW1lbnQuIEFsdGVybmF0aXZlIHRvIGBmbGlwYC5cbiAqIEBzZWUgaHR0cHM6Ly9mbG9hdGluZy11aS5jb20vZG9jcy9hdXRvUGxhY2VtZW50XG4gKi9cbmNvbnN0IGF1dG9QbGFjZW1lbnQgPSBhdXRvUGxhY2VtZW50JDE7XG5cbi8qKlxuICogT3B0aW1pemVzIHRoZSB2aXNpYmlsaXR5IG9mIHRoZSBmbG9hdGluZyBlbGVtZW50IGJ5IHNoaWZ0aW5nIGl0IGluIG9yZGVyIHRvXG4gKiBrZWVwIGl0IGluIHZpZXcgd2hlbiBpdCB3aWxsIG92ZXJmbG93IHRoZSBjbGlwcGluZyBib3VuZGFyeS5cbiAqIEBzZWUgaHR0cHM6Ly9mbG9hdGluZy11aS5jb20vZG9jcy9zaGlmdFxuICovXG5jb25zdCBzaGlmdCA9IHNoaWZ0JDE7XG5cbi8qKlxuICogT3B0aW1pemVzIHRoZSB2aXNpYmlsaXR5IG9mIHRoZSBmbG9hdGluZyBlbGVtZW50IGJ5IGZsaXBwaW5nIHRoZSBgcGxhY2VtZW50YFxuICogaW4gb3JkZXIgdG8ga2VlcCBpdCBpbiB2aWV3IHdoZW4gdGhlIHByZWZlcnJlZCBwbGFjZW1lbnQocykgd2lsbCBvdmVyZmxvdyB0aGVcbiAqIGNsaXBwaW5nIGJvdW5kYXJ5LiBBbHRlcm5hdGl2ZSB0byBgYXV0b1BsYWNlbWVudGAuXG4gKiBAc2VlIGh0dHBzOi8vZmxvYXRpbmctdWkuY29tL2RvY3MvZmxpcFxuICovXG5jb25zdCBmbGlwID0gZmxpcCQxO1xuXG4vKipcbiAqIFByb3ZpZGVzIGRhdGEgdGhhdCBhbGxvd3MgeW91IHRvIGNoYW5nZSB0aGUgc2l6ZSBvZiB0aGUgZmxvYXRpbmcgZWxlbWVudCDigJRcbiAqIGZvciBpbnN0YW5jZSwgcHJldmVudCBpdCBmcm9tIG92ZXJmbG93aW5nIHRoZSBjbGlwcGluZyBib3VuZGFyeSBvciBtYXRjaCB0aGVcbiAqIHdpZHRoIG9mIHRoZSByZWZlcmVuY2UgZWxlbWVudC5cbiAqIEBzZWUgaHR0cHM6Ly9mbG9hdGluZy11aS5jb20vZG9jcy9zaXplXG4gKi9cbmNvbnN0IHNpemUgPSBzaXplJDE7XG5cbi8qKlxuICogUHJvdmlkZXMgZGF0YSB0byBoaWRlIHRoZSBmbG9hdGluZyBlbGVtZW50IGluIGFwcGxpY2FibGUgc2l0dWF0aW9ucywgc3VjaCBhc1xuICogd2hlbiBpdCBpcyBub3QgaW4gdGhlIHNhbWUgY2xpcHBpbmcgY29udGV4dCBhcyB0aGUgcmVmZXJlbmNlIGVsZW1lbnQuXG4gKiBAc2VlIGh0dHBzOi8vZmxvYXRpbmctdWkuY29tL2RvY3MvaGlkZVxuICovXG5jb25zdCBoaWRlID0gaGlkZSQxO1xuXG4vKipcbiAqIFByb3ZpZGVzIGRhdGEgdG8gcG9zaXRpb24gYW4gaW5uZXIgZWxlbWVudCBvZiB0aGUgZmxvYXRpbmcgZWxlbWVudCBzbyB0aGF0IGl0XG4gKiBhcHBlYXJzIGNlbnRlcmVkIHRvIHRoZSByZWZlcmVuY2UgZWxlbWVudC5cbiAqIEBzZWUgaHR0cHM6Ly9mbG9hdGluZy11aS5jb20vZG9jcy9hcnJvd1xuICovXG5jb25zdCBhcnJvdyA9IGFycm93JDE7XG5cbi8qKlxuICogUHJvdmlkZXMgaW1wcm92ZWQgcG9zaXRpb25pbmcgZm9yIGlubGluZSByZWZlcmVuY2UgZWxlbWVudHMgdGhhdCBjYW4gc3BhblxuICogb3ZlciBtdWx0aXBsZSBsaW5lcywgc3VjaCBhcyBoeXBlcmxpbmtzIG9yIHJhbmdlIHNlbGVjdGlvbnMuXG4gKiBAc2VlIGh0dHBzOi8vZmxvYXRpbmctdWkuY29tL2RvY3MvaW5saW5lXG4gKi9cbmNvbnN0IGlubGluZSA9IGlubGluZSQxO1xuXG4vKipcbiAqIEJ1aWx0LWluIGBsaW1pdGVyYCB0aGF0IHdpbGwgc3RvcCBgc2hpZnQoKWAgYXQgYSBjZXJ0YWluIHBvaW50LlxuICovXG5jb25zdCBsaW1pdFNoaWZ0ID0gbGltaXRTaGlmdCQxO1xuXG4vKipcbiAqIENvbXB1dGVzIHRoZSBgeGAgYW5kIGB5YCBjb29yZGluYXRlcyB0aGF0IHdpbGwgcGxhY2UgdGhlIGZsb2F0aW5nIGVsZW1lbnRcbiAqIG5leHQgdG8gYSBnaXZlbiByZWZlcmVuY2UgZWxlbWVudC5cbiAqL1xuY29uc3QgY29tcHV0ZVBvc2l0aW9uID0gKHJlZmVyZW5jZSwgZmxvYXRpbmcsIG9wdGlvbnMpID0+IHtcbiAgLy8gVGhpcyBjYWNoZXMgdGhlIGV4cGVuc2l2ZSBgZ2V0Q2xpcHBpbmdFbGVtZW50QW5jZXN0b3JzYCBmdW5jdGlvbiBzbyB0aGF0XG4gIC8vIG11bHRpcGxlIGxpZmVjeWNsZSByZXNldHMgcmUtdXNlIHRoZSBzYW1lIHJlc3VsdC4gSXQgb25seSBsaXZlcyBmb3IgYVxuICAvLyBzaW5nbGUgY2FsbC4gSWYgb3RoZXIgZnVuY3Rpb25zIGJlY29tZSBleHBlbnNpdmUsIHdlIGNhbiBhZGQgdGhlbSBhcyB3ZWxsLlxuICBjb25zdCBjYWNoZSA9IG5ldyBNYXAoKTtcbiAgY29uc3QgbWVyZ2VkT3B0aW9ucyA9IHtcbiAgICBwbGF0Zm9ybSxcbiAgICAuLi5vcHRpb25zXG4gIH07XG4gIGNvbnN0IHBsYXRmb3JtV2l0aENhY2hlID0ge1xuICAgIC4uLm1lcmdlZE9wdGlvbnMucGxhdGZvcm0sXG4gICAgX2M6IGNhY2hlXG4gIH07XG4gIHJldHVybiBjb21wdXRlUG9zaXRpb24kMShyZWZlcmVuY2UsIGZsb2F0aW5nLCB7XG4gICAgLi4ubWVyZ2VkT3B0aW9ucyxcbiAgICBwbGF0Zm9ybTogcGxhdGZvcm1XaXRoQ2FjaGVcbiAgfSk7XG59O1xuXG5leHBvcnQgeyBhcnJvdywgYXV0b1BsYWNlbWVudCwgYXV0b1VwZGF0ZSwgY29tcHV0ZVBvc2l0aW9uLCBkZXRlY3RPdmVyZmxvdywgZmxpcCwgaGlkZSwgaW5saW5lLCBsaW1pdFNoaWZ0LCBvZmZzZXQsIHBsYXRmb3JtLCBzaGlmdCwgc2l6ZSB9O1xuIiwiLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL3NjaGVkdWxlci5qc1xudmFyIGZsdXNoUGVuZGluZyA9IGZhbHNlO1xudmFyIGZsdXNoaW5nID0gZmFsc2U7XG52YXIgcXVldWUgPSBbXTtcbmZ1bmN0aW9uIHNjaGVkdWxlcihjYWxsYmFjaykge1xuICBxdWV1ZUpvYihjYWxsYmFjayk7XG59XG5mdW5jdGlvbiBxdWV1ZUpvYihqb2IpIHtcbiAgaWYgKCFxdWV1ZS5pbmNsdWRlcyhqb2IpKVxuICAgIHF1ZXVlLnB1c2goam9iKTtcbiAgcXVldWVGbHVzaCgpO1xufVxuZnVuY3Rpb24gZGVxdWV1ZUpvYihqb2IpIHtcbiAgbGV0IGluZGV4ID0gcXVldWUuaW5kZXhPZihqb2IpO1xuICBpZiAoaW5kZXggIT09IC0xKVxuICAgIHF1ZXVlLnNwbGljZShpbmRleCwgMSk7XG59XG5mdW5jdGlvbiBxdWV1ZUZsdXNoKCkge1xuICBpZiAoIWZsdXNoaW5nICYmICFmbHVzaFBlbmRpbmcpIHtcbiAgICBmbHVzaFBlbmRpbmcgPSB0cnVlO1xuICAgIHF1ZXVlTWljcm90YXNrKGZsdXNoSm9icyk7XG4gIH1cbn1cbmZ1bmN0aW9uIGZsdXNoSm9icygpIHtcbiAgZmx1c2hQZW5kaW5nID0gZmFsc2U7XG4gIGZsdXNoaW5nID0gdHJ1ZTtcbiAgZm9yIChsZXQgaSA9IDA7IGkgPCBxdWV1ZS5sZW5ndGg7IGkrKykge1xuICAgIHF1ZXVlW2ldKCk7XG4gIH1cbiAgcXVldWUubGVuZ3RoID0gMDtcbiAgZmx1c2hpbmcgPSBmYWxzZTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL3JlYWN0aXZpdHkuanNcbnZhciByZWFjdGl2ZTtcbnZhciBlZmZlY3Q7XG52YXIgcmVsZWFzZTtcbnZhciByYXc7XG52YXIgc2hvdWxkU2NoZWR1bGUgPSB0cnVlO1xuZnVuY3Rpb24gZGlzYWJsZUVmZmVjdFNjaGVkdWxpbmcoY2FsbGJhY2spIHtcbiAgc2hvdWxkU2NoZWR1bGUgPSBmYWxzZTtcbiAgY2FsbGJhY2soKTtcbiAgc2hvdWxkU2NoZWR1bGUgPSB0cnVlO1xufVxuZnVuY3Rpb24gc2V0UmVhY3Rpdml0eUVuZ2luZShlbmdpbmUpIHtcbiAgcmVhY3RpdmUgPSBlbmdpbmUucmVhY3RpdmU7XG4gIHJlbGVhc2UgPSBlbmdpbmUucmVsZWFzZTtcbiAgZWZmZWN0ID0gKGNhbGxiYWNrKSA9PiBlbmdpbmUuZWZmZWN0KGNhbGxiYWNrLCB7c2NoZWR1bGVyOiAodGFzaykgPT4ge1xuICAgIGlmIChzaG91bGRTY2hlZHVsZSkge1xuICAgICAgc2NoZWR1bGVyKHRhc2spO1xuICAgIH0gZWxzZSB7XG4gICAgICB0YXNrKCk7XG4gICAgfVxuICB9fSk7XG4gIHJhdyA9IGVuZ2luZS5yYXc7XG59XG5mdW5jdGlvbiBvdmVycmlkZUVmZmVjdChvdmVycmlkZSkge1xuICBlZmZlY3QgPSBvdmVycmlkZTtcbn1cbmZ1bmN0aW9uIGVsZW1lbnRCb3VuZEVmZmVjdChlbCkge1xuICBsZXQgY2xlYW51cDIgPSAoKSA9PiB7XG4gIH07XG4gIGxldCB3cmFwcGVkRWZmZWN0ID0gKGNhbGxiYWNrKSA9PiB7XG4gICAgbGV0IGVmZmVjdFJlZmVyZW5jZSA9IGVmZmVjdChjYWxsYmFjayk7XG4gICAgaWYgKCFlbC5feF9lZmZlY3RzKSB7XG4gICAgICBlbC5feF9lZmZlY3RzID0gbmV3IFNldCgpO1xuICAgICAgZWwuX3hfcnVuRWZmZWN0cyA9ICgpID0+IHtcbiAgICAgICAgZWwuX3hfZWZmZWN0cy5mb3JFYWNoKChpKSA9PiBpKCkpO1xuICAgICAgfTtcbiAgICB9XG4gICAgZWwuX3hfZWZmZWN0cy5hZGQoZWZmZWN0UmVmZXJlbmNlKTtcbiAgICBjbGVhbnVwMiA9ICgpID0+IHtcbiAgICAgIGlmIChlZmZlY3RSZWZlcmVuY2UgPT09IHZvaWQgMClcbiAgICAgICAgcmV0dXJuO1xuICAgICAgZWwuX3hfZWZmZWN0cy5kZWxldGUoZWZmZWN0UmVmZXJlbmNlKTtcbiAgICAgIHJlbGVhc2UoZWZmZWN0UmVmZXJlbmNlKTtcbiAgICB9O1xuICAgIHJldHVybiBlZmZlY3RSZWZlcmVuY2U7XG4gIH07XG4gIHJldHVybiBbd3JhcHBlZEVmZmVjdCwgKCkgPT4ge1xuICAgIGNsZWFudXAyKCk7XG4gIH1dO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvbXV0YXRpb24uanNcbnZhciBvbkF0dHJpYnV0ZUFkZGVkcyA9IFtdO1xudmFyIG9uRWxSZW1vdmVkcyA9IFtdO1xudmFyIG9uRWxBZGRlZHMgPSBbXTtcbmZ1bmN0aW9uIG9uRWxBZGRlZChjYWxsYmFjaykge1xuICBvbkVsQWRkZWRzLnB1c2goY2FsbGJhY2spO1xufVxuZnVuY3Rpb24gb25FbFJlbW92ZWQoZWwsIGNhbGxiYWNrKSB7XG4gIGlmICh0eXBlb2YgY2FsbGJhY2sgPT09IFwiZnVuY3Rpb25cIikge1xuICAgIGlmICghZWwuX3hfY2xlYW51cHMpXG4gICAgICBlbC5feF9jbGVhbnVwcyA9IFtdO1xuICAgIGVsLl94X2NsZWFudXBzLnB1c2goY2FsbGJhY2spO1xuICB9IGVsc2Uge1xuICAgIGNhbGxiYWNrID0gZWw7XG4gICAgb25FbFJlbW92ZWRzLnB1c2goY2FsbGJhY2spO1xuICB9XG59XG5mdW5jdGlvbiBvbkF0dHJpYnV0ZXNBZGRlZChjYWxsYmFjaykge1xuICBvbkF0dHJpYnV0ZUFkZGVkcy5wdXNoKGNhbGxiYWNrKTtcbn1cbmZ1bmN0aW9uIG9uQXR0cmlidXRlUmVtb3ZlZChlbCwgbmFtZSwgY2FsbGJhY2spIHtcbiAgaWYgKCFlbC5feF9hdHRyaWJ1dGVDbGVhbnVwcylcbiAgICBlbC5feF9hdHRyaWJ1dGVDbGVhbnVwcyA9IHt9O1xuICBpZiAoIWVsLl94X2F0dHJpYnV0ZUNsZWFudXBzW25hbWVdKVxuICAgIGVsLl94X2F0dHJpYnV0ZUNsZWFudXBzW25hbWVdID0gW107XG4gIGVsLl94X2F0dHJpYnV0ZUNsZWFudXBzW25hbWVdLnB1c2goY2FsbGJhY2spO1xufVxuZnVuY3Rpb24gY2xlYW51cEF0dHJpYnV0ZXMoZWwsIG5hbWVzKSB7XG4gIGlmICghZWwuX3hfYXR0cmlidXRlQ2xlYW51cHMpXG4gICAgcmV0dXJuO1xuICBPYmplY3QuZW50cmllcyhlbC5feF9hdHRyaWJ1dGVDbGVhbnVwcykuZm9yRWFjaCgoW25hbWUsIHZhbHVlXSkgPT4ge1xuICAgIGlmIChuYW1lcyA9PT0gdm9pZCAwIHx8IG5hbWVzLmluY2x1ZGVzKG5hbWUpKSB7XG4gICAgICB2YWx1ZS5mb3JFYWNoKChpKSA9PiBpKCkpO1xuICAgICAgZGVsZXRlIGVsLl94X2F0dHJpYnV0ZUNsZWFudXBzW25hbWVdO1xuICAgIH1cbiAgfSk7XG59XG52YXIgb2JzZXJ2ZXIgPSBuZXcgTXV0YXRpb25PYnNlcnZlcihvbk11dGF0ZSk7XG52YXIgY3VycmVudGx5T2JzZXJ2aW5nID0gZmFsc2U7XG5mdW5jdGlvbiBzdGFydE9ic2VydmluZ011dGF0aW9ucygpIHtcbiAgb2JzZXJ2ZXIub2JzZXJ2ZShkb2N1bWVudCwge3N1YnRyZWU6IHRydWUsIGNoaWxkTGlzdDogdHJ1ZSwgYXR0cmlidXRlczogdHJ1ZSwgYXR0cmlidXRlT2xkVmFsdWU6IHRydWV9KTtcbiAgY3VycmVudGx5T2JzZXJ2aW5nID0gdHJ1ZTtcbn1cbmZ1bmN0aW9uIHN0b3BPYnNlcnZpbmdNdXRhdGlvbnMoKSB7XG4gIGZsdXNoT2JzZXJ2ZXIoKTtcbiAgb2JzZXJ2ZXIuZGlzY29ubmVjdCgpO1xuICBjdXJyZW50bHlPYnNlcnZpbmcgPSBmYWxzZTtcbn1cbnZhciByZWNvcmRRdWV1ZSA9IFtdO1xudmFyIHdpbGxQcm9jZXNzUmVjb3JkUXVldWUgPSBmYWxzZTtcbmZ1bmN0aW9uIGZsdXNoT2JzZXJ2ZXIoKSB7XG4gIHJlY29yZFF1ZXVlID0gcmVjb3JkUXVldWUuY29uY2F0KG9ic2VydmVyLnRha2VSZWNvcmRzKCkpO1xuICBpZiAocmVjb3JkUXVldWUubGVuZ3RoICYmICF3aWxsUHJvY2Vzc1JlY29yZFF1ZXVlKSB7XG4gICAgd2lsbFByb2Nlc3NSZWNvcmRRdWV1ZSA9IHRydWU7XG4gICAgcXVldWVNaWNyb3Rhc2soKCkgPT4ge1xuICAgICAgcHJvY2Vzc1JlY29yZFF1ZXVlKCk7XG4gICAgICB3aWxsUHJvY2Vzc1JlY29yZFF1ZXVlID0gZmFsc2U7XG4gICAgfSk7XG4gIH1cbn1cbmZ1bmN0aW9uIHByb2Nlc3NSZWNvcmRRdWV1ZSgpIHtcbiAgb25NdXRhdGUocmVjb3JkUXVldWUpO1xuICByZWNvcmRRdWV1ZS5sZW5ndGggPSAwO1xufVxuZnVuY3Rpb24gbXV0YXRlRG9tKGNhbGxiYWNrKSB7XG4gIGlmICghY3VycmVudGx5T2JzZXJ2aW5nKVxuICAgIHJldHVybiBjYWxsYmFjaygpO1xuICBzdG9wT2JzZXJ2aW5nTXV0YXRpb25zKCk7XG4gIGxldCByZXN1bHQgPSBjYWxsYmFjaygpO1xuICBzdGFydE9ic2VydmluZ011dGF0aW9ucygpO1xuICByZXR1cm4gcmVzdWx0O1xufVxudmFyIGlzQ29sbGVjdGluZyA9IGZhbHNlO1xudmFyIGRlZmVycmVkTXV0YXRpb25zID0gW107XG5mdW5jdGlvbiBkZWZlck11dGF0aW9ucygpIHtcbiAgaXNDb2xsZWN0aW5nID0gdHJ1ZTtcbn1cbmZ1bmN0aW9uIGZsdXNoQW5kU3RvcERlZmVycmluZ011dGF0aW9ucygpIHtcbiAgaXNDb2xsZWN0aW5nID0gZmFsc2U7XG4gIG9uTXV0YXRlKGRlZmVycmVkTXV0YXRpb25zKTtcbiAgZGVmZXJyZWRNdXRhdGlvbnMgPSBbXTtcbn1cbmZ1bmN0aW9uIG9uTXV0YXRlKG11dGF0aW9ucykge1xuICBpZiAoaXNDb2xsZWN0aW5nKSB7XG4gICAgZGVmZXJyZWRNdXRhdGlvbnMgPSBkZWZlcnJlZE11dGF0aW9ucy5jb25jYXQobXV0YXRpb25zKTtcbiAgICByZXR1cm47XG4gIH1cbiAgbGV0IGFkZGVkTm9kZXMgPSBbXTtcbiAgbGV0IHJlbW92ZWROb2RlcyA9IFtdO1xuICBsZXQgYWRkZWRBdHRyaWJ1dGVzID0gbmV3IE1hcCgpO1xuICBsZXQgcmVtb3ZlZEF0dHJpYnV0ZXMgPSBuZXcgTWFwKCk7XG4gIGZvciAobGV0IGkgPSAwOyBpIDwgbXV0YXRpb25zLmxlbmd0aDsgaSsrKSB7XG4gICAgaWYgKG11dGF0aW9uc1tpXS50YXJnZXQuX3hfaWdub3JlTXV0YXRpb25PYnNlcnZlcilcbiAgICAgIGNvbnRpbnVlO1xuICAgIGlmIChtdXRhdGlvbnNbaV0udHlwZSA9PT0gXCJjaGlsZExpc3RcIikge1xuICAgICAgbXV0YXRpb25zW2ldLmFkZGVkTm9kZXMuZm9yRWFjaCgobm9kZSkgPT4gbm9kZS5ub2RlVHlwZSA9PT0gMSAmJiBhZGRlZE5vZGVzLnB1c2gobm9kZSkpO1xuICAgICAgbXV0YXRpb25zW2ldLnJlbW92ZWROb2Rlcy5mb3JFYWNoKChub2RlKSA9PiBub2RlLm5vZGVUeXBlID09PSAxICYmIHJlbW92ZWROb2Rlcy5wdXNoKG5vZGUpKTtcbiAgICB9XG4gICAgaWYgKG11dGF0aW9uc1tpXS50eXBlID09PSBcImF0dHJpYnV0ZXNcIikge1xuICAgICAgbGV0IGVsID0gbXV0YXRpb25zW2ldLnRhcmdldDtcbiAgICAgIGxldCBuYW1lID0gbXV0YXRpb25zW2ldLmF0dHJpYnV0ZU5hbWU7XG4gICAgICBsZXQgb2xkVmFsdWUgPSBtdXRhdGlvbnNbaV0ub2xkVmFsdWU7XG4gICAgICBsZXQgYWRkMiA9ICgpID0+IHtcbiAgICAgICAgaWYgKCFhZGRlZEF0dHJpYnV0ZXMuaGFzKGVsKSlcbiAgICAgICAgICBhZGRlZEF0dHJpYnV0ZXMuc2V0KGVsLCBbXSk7XG4gICAgICAgIGFkZGVkQXR0cmlidXRlcy5nZXQoZWwpLnB1c2goe25hbWUsIHZhbHVlOiBlbC5nZXRBdHRyaWJ1dGUobmFtZSl9KTtcbiAgICAgIH07XG4gICAgICBsZXQgcmVtb3ZlID0gKCkgPT4ge1xuICAgICAgICBpZiAoIXJlbW92ZWRBdHRyaWJ1dGVzLmhhcyhlbCkpXG4gICAgICAgICAgcmVtb3ZlZEF0dHJpYnV0ZXMuc2V0KGVsLCBbXSk7XG4gICAgICAgIHJlbW92ZWRBdHRyaWJ1dGVzLmdldChlbCkucHVzaChuYW1lKTtcbiAgICAgIH07XG4gICAgICBpZiAoZWwuaGFzQXR0cmlidXRlKG5hbWUpICYmIG9sZFZhbHVlID09PSBudWxsKSB7XG4gICAgICAgIGFkZDIoKTtcbiAgICAgIH0gZWxzZSBpZiAoZWwuaGFzQXR0cmlidXRlKG5hbWUpKSB7XG4gICAgICAgIHJlbW92ZSgpO1xuICAgICAgICBhZGQyKCk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICByZW1vdmUoKTtcbiAgICAgIH1cbiAgICB9XG4gIH1cbiAgcmVtb3ZlZEF0dHJpYnV0ZXMuZm9yRWFjaCgoYXR0cnMsIGVsKSA9PiB7XG4gICAgY2xlYW51cEF0dHJpYnV0ZXMoZWwsIGF0dHJzKTtcbiAgfSk7XG4gIGFkZGVkQXR0cmlidXRlcy5mb3JFYWNoKChhdHRycywgZWwpID0+IHtcbiAgICBvbkF0dHJpYnV0ZUFkZGVkcy5mb3JFYWNoKChpKSA9PiBpKGVsLCBhdHRycykpO1xuICB9KTtcbiAgZm9yIChsZXQgbm9kZSBvZiByZW1vdmVkTm9kZXMpIHtcbiAgICBpZiAoYWRkZWROb2Rlcy5pbmNsdWRlcyhub2RlKSlcbiAgICAgIGNvbnRpbnVlO1xuICAgIG9uRWxSZW1vdmVkcy5mb3JFYWNoKChpKSA9PiBpKG5vZGUpKTtcbiAgICBpZiAobm9kZS5feF9jbGVhbnVwcykge1xuICAgICAgd2hpbGUgKG5vZGUuX3hfY2xlYW51cHMubGVuZ3RoKVxuICAgICAgICBub2RlLl94X2NsZWFudXBzLnBvcCgpKCk7XG4gICAgfVxuICB9XG4gIGFkZGVkTm9kZXMuZm9yRWFjaCgobm9kZSkgPT4ge1xuICAgIG5vZGUuX3hfaWdub3JlU2VsZiA9IHRydWU7XG4gICAgbm9kZS5feF9pZ25vcmUgPSB0cnVlO1xuICB9KTtcbiAgZm9yIChsZXQgbm9kZSBvZiBhZGRlZE5vZGVzKSB7XG4gICAgaWYgKHJlbW92ZWROb2Rlcy5pbmNsdWRlcyhub2RlKSlcbiAgICAgIGNvbnRpbnVlO1xuICAgIGlmICghbm9kZS5pc0Nvbm5lY3RlZClcbiAgICAgIGNvbnRpbnVlO1xuICAgIGRlbGV0ZSBub2RlLl94X2lnbm9yZVNlbGY7XG4gICAgZGVsZXRlIG5vZGUuX3hfaWdub3JlO1xuICAgIG9uRWxBZGRlZHMuZm9yRWFjaCgoaSkgPT4gaShub2RlKSk7XG4gICAgbm9kZS5feF9pZ25vcmUgPSB0cnVlO1xuICAgIG5vZGUuX3hfaWdub3JlU2VsZiA9IHRydWU7XG4gIH1cbiAgYWRkZWROb2Rlcy5mb3JFYWNoKChub2RlKSA9PiB7XG4gICAgZGVsZXRlIG5vZGUuX3hfaWdub3JlU2VsZjtcbiAgICBkZWxldGUgbm9kZS5feF9pZ25vcmU7XG4gIH0pO1xuICBhZGRlZE5vZGVzID0gbnVsbDtcbiAgcmVtb3ZlZE5vZGVzID0gbnVsbDtcbiAgYWRkZWRBdHRyaWJ1dGVzID0gbnVsbDtcbiAgcmVtb3ZlZEF0dHJpYnV0ZXMgPSBudWxsO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvc2NvcGUuanNcbmZ1bmN0aW9uIHNjb3BlKG5vZGUpIHtcbiAgcmV0dXJuIG1lcmdlUHJveGllcyhjbG9zZXN0RGF0YVN0YWNrKG5vZGUpKTtcbn1cbmZ1bmN0aW9uIGFkZFNjb3BlVG9Ob2RlKG5vZGUsIGRhdGEyLCByZWZlcmVuY2VOb2RlKSB7XG4gIG5vZGUuX3hfZGF0YVN0YWNrID0gW2RhdGEyLCAuLi5jbG9zZXN0RGF0YVN0YWNrKHJlZmVyZW5jZU5vZGUgfHwgbm9kZSldO1xuICByZXR1cm4gKCkgPT4ge1xuICAgIG5vZGUuX3hfZGF0YVN0YWNrID0gbm9kZS5feF9kYXRhU3RhY2suZmlsdGVyKChpKSA9PiBpICE9PSBkYXRhMik7XG4gIH07XG59XG5mdW5jdGlvbiByZWZyZXNoU2NvcGUoZWxlbWVudCwgc2NvcGUyKSB7XG4gIGxldCBleGlzdGluZ1Njb3BlID0gZWxlbWVudC5feF9kYXRhU3RhY2tbMF07XG4gIE9iamVjdC5lbnRyaWVzKHNjb3BlMikuZm9yRWFjaCgoW2tleSwgdmFsdWVdKSA9PiB7XG4gICAgZXhpc3RpbmdTY29wZVtrZXldID0gdmFsdWU7XG4gIH0pO1xufVxuZnVuY3Rpb24gY2xvc2VzdERhdGFTdGFjayhub2RlKSB7XG4gIGlmIChub2RlLl94X2RhdGFTdGFjaylcbiAgICByZXR1cm4gbm9kZS5feF9kYXRhU3RhY2s7XG4gIGlmICh0eXBlb2YgU2hhZG93Um9vdCA9PT0gXCJmdW5jdGlvblwiICYmIG5vZGUgaW5zdGFuY2VvZiBTaGFkb3dSb290KSB7XG4gICAgcmV0dXJuIGNsb3Nlc3REYXRhU3RhY2sobm9kZS5ob3N0KTtcbiAgfVxuICBpZiAoIW5vZGUucGFyZW50Tm9kZSkge1xuICAgIHJldHVybiBbXTtcbiAgfVxuICByZXR1cm4gY2xvc2VzdERhdGFTdGFjayhub2RlLnBhcmVudE5vZGUpO1xufVxuZnVuY3Rpb24gbWVyZ2VQcm94aWVzKG9iamVjdHMpIHtcbiAgbGV0IHRoaXNQcm94eSA9IG5ldyBQcm94eSh7fSwge1xuICAgIG93bktleXM6ICgpID0+IHtcbiAgICAgIHJldHVybiBBcnJheS5mcm9tKG5ldyBTZXQob2JqZWN0cy5mbGF0TWFwKChpKSA9PiBPYmplY3Qua2V5cyhpKSkpKTtcbiAgICB9LFxuICAgIGhhczogKHRhcmdldCwgbmFtZSkgPT4ge1xuICAgICAgcmV0dXJuIG9iamVjdHMuc29tZSgob2JqKSA9PiBvYmouaGFzT3duUHJvcGVydHkobmFtZSkpO1xuICAgIH0sXG4gICAgZ2V0OiAodGFyZ2V0LCBuYW1lKSA9PiB7XG4gICAgICByZXR1cm4gKG9iamVjdHMuZmluZCgob2JqKSA9PiB7XG4gICAgICAgIGlmIChvYmouaGFzT3duUHJvcGVydHkobmFtZSkpIHtcbiAgICAgICAgICBsZXQgZGVzY3JpcHRvciA9IE9iamVjdC5nZXRPd25Qcm9wZXJ0eURlc2NyaXB0b3Iob2JqLCBuYW1lKTtcbiAgICAgICAgICBpZiAoZGVzY3JpcHRvci5nZXQgJiYgZGVzY3JpcHRvci5nZXQuX3hfYWxyZWFkeUJvdW5kIHx8IGRlc2NyaXB0b3Iuc2V0ICYmIGRlc2NyaXB0b3Iuc2V0Ll94X2FscmVhZHlCb3VuZCkge1xuICAgICAgICAgICAgcmV0dXJuIHRydWU7XG4gICAgICAgICAgfVxuICAgICAgICAgIGlmICgoZGVzY3JpcHRvci5nZXQgfHwgZGVzY3JpcHRvci5zZXQpICYmIGRlc2NyaXB0b3IuZW51bWVyYWJsZSkge1xuICAgICAgICAgICAgbGV0IGdldHRlciA9IGRlc2NyaXB0b3IuZ2V0O1xuICAgICAgICAgICAgbGV0IHNldHRlciA9IGRlc2NyaXB0b3Iuc2V0O1xuICAgICAgICAgICAgbGV0IHByb3BlcnR5ID0gZGVzY3JpcHRvcjtcbiAgICAgICAgICAgIGdldHRlciA9IGdldHRlciAmJiBnZXR0ZXIuYmluZCh0aGlzUHJveHkpO1xuICAgICAgICAgICAgc2V0dGVyID0gc2V0dGVyICYmIHNldHRlci5iaW5kKHRoaXNQcm94eSk7XG4gICAgICAgICAgICBpZiAoZ2V0dGVyKVxuICAgICAgICAgICAgICBnZXR0ZXIuX3hfYWxyZWFkeUJvdW5kID0gdHJ1ZTtcbiAgICAgICAgICAgIGlmIChzZXR0ZXIpXG4gICAgICAgICAgICAgIHNldHRlci5feF9hbHJlYWR5Qm91bmQgPSB0cnVlO1xuICAgICAgICAgICAgT2JqZWN0LmRlZmluZVByb3BlcnR5KG9iaiwgbmFtZSwge1xuICAgICAgICAgICAgICAuLi5wcm9wZXJ0eSxcbiAgICAgICAgICAgICAgZ2V0OiBnZXR0ZXIsXG4gICAgICAgICAgICAgIHNldDogc2V0dGVyXG4gICAgICAgICAgICB9KTtcbiAgICAgICAgICB9XG4gICAgICAgICAgcmV0dXJuIHRydWU7XG4gICAgICAgIH1cbiAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgfSkgfHwge30pW25hbWVdO1xuICAgIH0sXG4gICAgc2V0OiAodGFyZ2V0LCBuYW1lLCB2YWx1ZSkgPT4ge1xuICAgICAgbGV0IGNsb3Nlc3RPYmplY3RXaXRoS2V5ID0gb2JqZWN0cy5maW5kKChvYmopID0+IG9iai5oYXNPd25Qcm9wZXJ0eShuYW1lKSk7XG4gICAgICBpZiAoY2xvc2VzdE9iamVjdFdpdGhLZXkpIHtcbiAgICAgICAgY2xvc2VzdE9iamVjdFdpdGhLZXlbbmFtZV0gPSB2YWx1ZTtcbiAgICAgIH0gZWxzZSB7XG4gICAgICAgIG9iamVjdHNbb2JqZWN0cy5sZW5ndGggLSAxXVtuYW1lXSA9IHZhbHVlO1xuICAgICAgfVxuICAgICAgcmV0dXJuIHRydWU7XG4gICAgfVxuICB9KTtcbiAgcmV0dXJuIHRoaXNQcm94eTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL2ludGVyY2VwdG9yLmpzXG5mdW5jdGlvbiBpbml0SW50ZXJjZXB0b3JzKGRhdGEyKSB7XG4gIGxldCBpc09iamVjdDIgPSAodmFsKSA9PiB0eXBlb2YgdmFsID09PSBcIm9iamVjdFwiICYmICFBcnJheS5pc0FycmF5KHZhbCkgJiYgdmFsICE9PSBudWxsO1xuICBsZXQgcmVjdXJzZSA9IChvYmosIGJhc2VQYXRoID0gXCJcIikgPT4ge1xuICAgIE9iamVjdC5lbnRyaWVzKE9iamVjdC5nZXRPd25Qcm9wZXJ0eURlc2NyaXB0b3JzKG9iaikpLmZvckVhY2goKFtrZXksIHt2YWx1ZSwgZW51bWVyYWJsZX1dKSA9PiB7XG4gICAgICBpZiAoZW51bWVyYWJsZSA9PT0gZmFsc2UgfHwgdmFsdWUgPT09IHZvaWQgMClcbiAgICAgICAgcmV0dXJuO1xuICAgICAgbGV0IHBhdGggPSBiYXNlUGF0aCA9PT0gXCJcIiA/IGtleSA6IGAke2Jhc2VQYXRofS4ke2tleX1gO1xuICAgICAgaWYgKHR5cGVvZiB2YWx1ZSA9PT0gXCJvYmplY3RcIiAmJiB2YWx1ZSAhPT0gbnVsbCAmJiB2YWx1ZS5feF9pbnRlcmNlcHRvcikge1xuICAgICAgICBvYmpba2V5XSA9IHZhbHVlLmluaXRpYWxpemUoZGF0YTIsIHBhdGgsIGtleSk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICBpZiAoaXNPYmplY3QyKHZhbHVlKSAmJiB2YWx1ZSAhPT0gb2JqICYmICEodmFsdWUgaW5zdGFuY2VvZiBFbGVtZW50KSkge1xuICAgICAgICAgIHJlY3Vyc2UodmFsdWUsIHBhdGgpO1xuICAgICAgICB9XG4gICAgICB9XG4gICAgfSk7XG4gIH07XG4gIHJldHVybiByZWN1cnNlKGRhdGEyKTtcbn1cbmZ1bmN0aW9uIGludGVyY2VwdG9yKGNhbGxiYWNrLCBtdXRhdGVPYmogPSAoKSA9PiB7XG59KSB7XG4gIGxldCBvYmogPSB7XG4gICAgaW5pdGlhbFZhbHVlOiB2b2lkIDAsXG4gICAgX3hfaW50ZXJjZXB0b3I6IHRydWUsXG4gICAgaW5pdGlhbGl6ZShkYXRhMiwgcGF0aCwga2V5KSB7XG4gICAgICByZXR1cm4gY2FsbGJhY2sodGhpcy5pbml0aWFsVmFsdWUsICgpID0+IGdldChkYXRhMiwgcGF0aCksICh2YWx1ZSkgPT4gc2V0KGRhdGEyLCBwYXRoLCB2YWx1ZSksIHBhdGgsIGtleSk7XG4gICAgfVxuICB9O1xuICBtdXRhdGVPYmoob2JqKTtcbiAgcmV0dXJuIChpbml0aWFsVmFsdWUpID0+IHtcbiAgICBpZiAodHlwZW9mIGluaXRpYWxWYWx1ZSA9PT0gXCJvYmplY3RcIiAmJiBpbml0aWFsVmFsdWUgIT09IG51bGwgJiYgaW5pdGlhbFZhbHVlLl94X2ludGVyY2VwdG9yKSB7XG4gICAgICBsZXQgaW5pdGlhbGl6ZSA9IG9iai5pbml0aWFsaXplLmJpbmQob2JqKTtcbiAgICAgIG9iai5pbml0aWFsaXplID0gKGRhdGEyLCBwYXRoLCBrZXkpID0+IHtcbiAgICAgICAgbGV0IGlubmVyVmFsdWUgPSBpbml0aWFsVmFsdWUuaW5pdGlhbGl6ZShkYXRhMiwgcGF0aCwga2V5KTtcbiAgICAgICAgb2JqLmluaXRpYWxWYWx1ZSA9IGlubmVyVmFsdWU7XG4gICAgICAgIHJldHVybiBpbml0aWFsaXplKGRhdGEyLCBwYXRoLCBrZXkpO1xuICAgICAgfTtcbiAgICB9IGVsc2Uge1xuICAgICAgb2JqLmluaXRpYWxWYWx1ZSA9IGluaXRpYWxWYWx1ZTtcbiAgICB9XG4gICAgcmV0dXJuIG9iajtcbiAgfTtcbn1cbmZ1bmN0aW9uIGdldChvYmosIHBhdGgpIHtcbiAgcmV0dXJuIHBhdGguc3BsaXQoXCIuXCIpLnJlZHVjZSgoY2FycnksIHNlZ21lbnQpID0+IGNhcnJ5W3NlZ21lbnRdLCBvYmopO1xufVxuZnVuY3Rpb24gc2V0KG9iaiwgcGF0aCwgdmFsdWUpIHtcbiAgaWYgKHR5cGVvZiBwYXRoID09PSBcInN0cmluZ1wiKVxuICAgIHBhdGggPSBwYXRoLnNwbGl0KFwiLlwiKTtcbiAgaWYgKHBhdGgubGVuZ3RoID09PSAxKVxuICAgIG9ialtwYXRoWzBdXSA9IHZhbHVlO1xuICBlbHNlIGlmIChwYXRoLmxlbmd0aCA9PT0gMClcbiAgICB0aHJvdyBlcnJvcjtcbiAgZWxzZSB7XG4gICAgaWYgKG9ialtwYXRoWzBdXSlcbiAgICAgIHJldHVybiBzZXQob2JqW3BhdGhbMF1dLCBwYXRoLnNsaWNlKDEpLCB2YWx1ZSk7XG4gICAgZWxzZSB7XG4gICAgICBvYmpbcGF0aFswXV0gPSB7fTtcbiAgICAgIHJldHVybiBzZXQob2JqW3BhdGhbMF1dLCBwYXRoLnNsaWNlKDEpLCB2YWx1ZSk7XG4gICAgfVxuICB9XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9tYWdpY3MuanNcbnZhciBtYWdpY3MgPSB7fTtcbmZ1bmN0aW9uIG1hZ2ljKG5hbWUsIGNhbGxiYWNrKSB7XG4gIG1hZ2ljc1tuYW1lXSA9IGNhbGxiYWNrO1xufVxuZnVuY3Rpb24gaW5qZWN0TWFnaWNzKG9iaiwgZWwpIHtcbiAgT2JqZWN0LmVudHJpZXMobWFnaWNzKS5mb3JFYWNoKChbbmFtZSwgY2FsbGJhY2tdKSA9PiB7XG4gICAgT2JqZWN0LmRlZmluZVByb3BlcnR5KG9iaiwgYCQke25hbWV9YCwge1xuICAgICAgZ2V0KCkge1xuICAgICAgICBsZXQgW3V0aWxpdGllcywgY2xlYW51cDJdID0gZ2V0RWxlbWVudEJvdW5kVXRpbGl0aWVzKGVsKTtcbiAgICAgICAgdXRpbGl0aWVzID0ge2ludGVyY2VwdG9yLCAuLi51dGlsaXRpZXN9O1xuICAgICAgICBvbkVsUmVtb3ZlZChlbCwgY2xlYW51cDIpO1xuICAgICAgICByZXR1cm4gY2FsbGJhY2soZWwsIHV0aWxpdGllcyk7XG4gICAgICB9LFxuICAgICAgZW51bWVyYWJsZTogZmFsc2VcbiAgICB9KTtcbiAgfSk7XG4gIHJldHVybiBvYmo7XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy91dGlscy9lcnJvci5qc1xuZnVuY3Rpb24gdHJ5Q2F0Y2goZWwsIGV4cHJlc3Npb24sIGNhbGxiYWNrLCAuLi5hcmdzKSB7XG4gIHRyeSB7XG4gICAgcmV0dXJuIGNhbGxiYWNrKC4uLmFyZ3MpO1xuICB9IGNhdGNoIChlKSB7XG4gICAgaGFuZGxlRXJyb3IoZSwgZWwsIGV4cHJlc3Npb24pO1xuICB9XG59XG5mdW5jdGlvbiBoYW5kbGVFcnJvcihlcnJvcjIsIGVsLCBleHByZXNzaW9uID0gdm9pZCAwKSB7XG4gIE9iamVjdC5hc3NpZ24oZXJyb3IyLCB7ZWwsIGV4cHJlc3Npb259KTtcbiAgY29uc29sZS53YXJuKGBBbHBpbmUgRXhwcmVzc2lvbiBFcnJvcjogJHtlcnJvcjIubWVzc2FnZX1cblxuJHtleHByZXNzaW9uID8gJ0V4cHJlc3Npb246IFwiJyArIGV4cHJlc3Npb24gKyAnXCJcXG5cXG4nIDogXCJcIn1gLCBlbCk7XG4gIHNldFRpbWVvdXQoKCkgPT4ge1xuICAgIHRocm93IGVycm9yMjtcbiAgfSwgMCk7XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9ldmFsdWF0b3IuanNcbnZhciBzaG91bGRBdXRvRXZhbHVhdGVGdW5jdGlvbnMgPSB0cnVlO1xuZnVuY3Rpb24gZG9udEF1dG9FdmFsdWF0ZUZ1bmN0aW9ucyhjYWxsYmFjaykge1xuICBsZXQgY2FjaGUgPSBzaG91bGRBdXRvRXZhbHVhdGVGdW5jdGlvbnM7XG4gIHNob3VsZEF1dG9FdmFsdWF0ZUZ1bmN0aW9ucyA9IGZhbHNlO1xuICBjYWxsYmFjaygpO1xuICBzaG91bGRBdXRvRXZhbHVhdGVGdW5jdGlvbnMgPSBjYWNoZTtcbn1cbmZ1bmN0aW9uIGV2YWx1YXRlKGVsLCBleHByZXNzaW9uLCBleHRyYXMgPSB7fSkge1xuICBsZXQgcmVzdWx0O1xuICBldmFsdWF0ZUxhdGVyKGVsLCBleHByZXNzaW9uKSgodmFsdWUpID0+IHJlc3VsdCA9IHZhbHVlLCBleHRyYXMpO1xuICByZXR1cm4gcmVzdWx0O1xufVxuZnVuY3Rpb24gZXZhbHVhdGVMYXRlciguLi5hcmdzKSB7XG4gIHJldHVybiB0aGVFdmFsdWF0b3JGdW5jdGlvbiguLi5hcmdzKTtcbn1cbnZhciB0aGVFdmFsdWF0b3JGdW5jdGlvbiA9IG5vcm1hbEV2YWx1YXRvcjtcbmZ1bmN0aW9uIHNldEV2YWx1YXRvcihuZXdFdmFsdWF0b3IpIHtcbiAgdGhlRXZhbHVhdG9yRnVuY3Rpb24gPSBuZXdFdmFsdWF0b3I7XG59XG5mdW5jdGlvbiBub3JtYWxFdmFsdWF0b3IoZWwsIGV4cHJlc3Npb24pIHtcbiAgbGV0IG92ZXJyaWRkZW5NYWdpY3MgPSB7fTtcbiAgaW5qZWN0TWFnaWNzKG92ZXJyaWRkZW5NYWdpY3MsIGVsKTtcbiAgbGV0IGRhdGFTdGFjayA9IFtvdmVycmlkZGVuTWFnaWNzLCAuLi5jbG9zZXN0RGF0YVN0YWNrKGVsKV07XG4gIGlmICh0eXBlb2YgZXhwcmVzc2lvbiA9PT0gXCJmdW5jdGlvblwiKSB7XG4gICAgcmV0dXJuIGdlbmVyYXRlRXZhbHVhdG9yRnJvbUZ1bmN0aW9uKGRhdGFTdGFjaywgZXhwcmVzc2lvbik7XG4gIH1cbiAgbGV0IGV2YWx1YXRvciA9IGdlbmVyYXRlRXZhbHVhdG9yRnJvbVN0cmluZyhkYXRhU3RhY2ssIGV4cHJlc3Npb24sIGVsKTtcbiAgcmV0dXJuIHRyeUNhdGNoLmJpbmQobnVsbCwgZWwsIGV4cHJlc3Npb24sIGV2YWx1YXRvcik7XG59XG5mdW5jdGlvbiBnZW5lcmF0ZUV2YWx1YXRvckZyb21GdW5jdGlvbihkYXRhU3RhY2ssIGZ1bmMpIHtcbiAgcmV0dXJuIChyZWNlaXZlciA9ICgpID0+IHtcbiAgfSwge3Njb3BlOiBzY29wZTIgPSB7fSwgcGFyYW1zID0gW119ID0ge30pID0+IHtcbiAgICBsZXQgcmVzdWx0ID0gZnVuYy5hcHBseShtZXJnZVByb3hpZXMoW3Njb3BlMiwgLi4uZGF0YVN0YWNrXSksIHBhcmFtcyk7XG4gICAgcnVuSWZUeXBlT2ZGdW5jdGlvbihyZWNlaXZlciwgcmVzdWx0KTtcbiAgfTtcbn1cbnZhciBldmFsdWF0b3JNZW1vID0ge307XG5mdW5jdGlvbiBnZW5lcmF0ZUZ1bmN0aW9uRnJvbVN0cmluZyhleHByZXNzaW9uLCBlbCkge1xuICBpZiAoZXZhbHVhdG9yTWVtb1tleHByZXNzaW9uXSkge1xuICAgIHJldHVybiBldmFsdWF0b3JNZW1vW2V4cHJlc3Npb25dO1xuICB9XG4gIGxldCBBc3luY0Z1bmN0aW9uID0gT2JqZWN0LmdldFByb3RvdHlwZU9mKGFzeW5jIGZ1bmN0aW9uKCkge1xuICB9KS5jb25zdHJ1Y3RvcjtcbiAgbGV0IHJpZ2h0U2lkZVNhZmVFeHByZXNzaW9uID0gL15bXFxuXFxzXSppZi4qXFwoLipcXCkvLnRlc3QoZXhwcmVzc2lvbikgfHwgL14obGV0fGNvbnN0KVxccy8udGVzdChleHByZXNzaW9uKSA/IGAoKCkgPT4geyAke2V4cHJlc3Npb259IH0pKClgIDogZXhwcmVzc2lvbjtcbiAgY29uc3Qgc2FmZUFzeW5jRnVuY3Rpb24gPSAoKSA9PiB7XG4gICAgdHJ5IHtcbiAgICAgIHJldHVybiBuZXcgQXN5bmNGdW5jdGlvbihbXCJfX3NlbGZcIiwgXCJzY29wZVwiXSwgYHdpdGggKHNjb3BlKSB7IF9fc2VsZi5yZXN1bHQgPSAke3JpZ2h0U2lkZVNhZmVFeHByZXNzaW9ufSB9OyBfX3NlbGYuZmluaXNoZWQgPSB0cnVlOyByZXR1cm4gX19zZWxmLnJlc3VsdDtgKTtcbiAgICB9IGNhdGNoIChlcnJvcjIpIHtcbiAgICAgIGhhbmRsZUVycm9yKGVycm9yMiwgZWwsIGV4cHJlc3Npb24pO1xuICAgICAgcmV0dXJuIFByb21pc2UucmVzb2x2ZSgpO1xuICAgIH1cbiAgfTtcbiAgbGV0IGZ1bmMgPSBzYWZlQXN5bmNGdW5jdGlvbigpO1xuICBldmFsdWF0b3JNZW1vW2V4cHJlc3Npb25dID0gZnVuYztcbiAgcmV0dXJuIGZ1bmM7XG59XG5mdW5jdGlvbiBnZW5lcmF0ZUV2YWx1YXRvckZyb21TdHJpbmcoZGF0YVN0YWNrLCBleHByZXNzaW9uLCBlbCkge1xuICBsZXQgZnVuYyA9IGdlbmVyYXRlRnVuY3Rpb25Gcm9tU3RyaW5nKGV4cHJlc3Npb24sIGVsKTtcbiAgcmV0dXJuIChyZWNlaXZlciA9ICgpID0+IHtcbiAgfSwge3Njb3BlOiBzY29wZTIgPSB7fSwgcGFyYW1zID0gW119ID0ge30pID0+IHtcbiAgICBmdW5jLnJlc3VsdCA9IHZvaWQgMDtcbiAgICBmdW5jLmZpbmlzaGVkID0gZmFsc2U7XG4gICAgbGV0IGNvbXBsZXRlU2NvcGUgPSBtZXJnZVByb3hpZXMoW3Njb3BlMiwgLi4uZGF0YVN0YWNrXSk7XG4gICAgaWYgKHR5cGVvZiBmdW5jID09PSBcImZ1bmN0aW9uXCIpIHtcbiAgICAgIGxldCBwcm9taXNlID0gZnVuYyhmdW5jLCBjb21wbGV0ZVNjb3BlKS5jYXRjaCgoZXJyb3IyKSA9PiBoYW5kbGVFcnJvcihlcnJvcjIsIGVsLCBleHByZXNzaW9uKSk7XG4gICAgICBpZiAoZnVuYy5maW5pc2hlZCkge1xuICAgICAgICBydW5JZlR5cGVPZkZ1bmN0aW9uKHJlY2VpdmVyLCBmdW5jLnJlc3VsdCwgY29tcGxldGVTY29wZSwgcGFyYW1zLCBlbCk7XG4gICAgICAgIGZ1bmMucmVzdWx0ID0gdm9pZCAwO1xuICAgICAgfSBlbHNlIHtcbiAgICAgICAgcHJvbWlzZS50aGVuKChyZXN1bHQpID0+IHtcbiAgICAgICAgICBydW5JZlR5cGVPZkZ1bmN0aW9uKHJlY2VpdmVyLCByZXN1bHQsIGNvbXBsZXRlU2NvcGUsIHBhcmFtcywgZWwpO1xuICAgICAgICB9KS5jYXRjaCgoZXJyb3IyKSA9PiBoYW5kbGVFcnJvcihlcnJvcjIsIGVsLCBleHByZXNzaW9uKSkuZmluYWxseSgoKSA9PiBmdW5jLnJlc3VsdCA9IHZvaWQgMCk7XG4gICAgICB9XG4gICAgfVxuICB9O1xufVxuZnVuY3Rpb24gcnVuSWZUeXBlT2ZGdW5jdGlvbihyZWNlaXZlciwgdmFsdWUsIHNjb3BlMiwgcGFyYW1zLCBlbCkge1xuICBpZiAoc2hvdWxkQXV0b0V2YWx1YXRlRnVuY3Rpb25zICYmIHR5cGVvZiB2YWx1ZSA9PT0gXCJmdW5jdGlvblwiKSB7XG4gICAgbGV0IHJlc3VsdCA9IHZhbHVlLmFwcGx5KHNjb3BlMiwgcGFyYW1zKTtcbiAgICBpZiAocmVzdWx0IGluc3RhbmNlb2YgUHJvbWlzZSkge1xuICAgICAgcmVzdWx0LnRoZW4oKGkpID0+IHJ1bklmVHlwZU9mRnVuY3Rpb24ocmVjZWl2ZXIsIGksIHNjb3BlMiwgcGFyYW1zKSkuY2F0Y2goKGVycm9yMikgPT4gaGFuZGxlRXJyb3IoZXJyb3IyLCBlbCwgdmFsdWUpKTtcbiAgICB9IGVsc2Uge1xuICAgICAgcmVjZWl2ZXIocmVzdWx0KTtcbiAgICB9XG4gIH0gZWxzZSB7XG4gICAgcmVjZWl2ZXIodmFsdWUpO1xuICB9XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9kaXJlY3RpdmVzLmpzXG52YXIgcHJlZml4QXNTdHJpbmcgPSBcIngtXCI7XG5mdW5jdGlvbiBwcmVmaXgoc3ViamVjdCA9IFwiXCIpIHtcbiAgcmV0dXJuIHByZWZpeEFzU3RyaW5nICsgc3ViamVjdDtcbn1cbmZ1bmN0aW9uIHNldFByZWZpeChuZXdQcmVmaXgpIHtcbiAgcHJlZml4QXNTdHJpbmcgPSBuZXdQcmVmaXg7XG59XG52YXIgZGlyZWN0aXZlSGFuZGxlcnMgPSB7fTtcbmZ1bmN0aW9uIGRpcmVjdGl2ZShuYW1lLCBjYWxsYmFjaykge1xuICBkaXJlY3RpdmVIYW5kbGVyc1tuYW1lXSA9IGNhbGxiYWNrO1xufVxuZnVuY3Rpb24gZGlyZWN0aXZlcyhlbCwgYXR0cmlidXRlcywgb3JpZ2luYWxBdHRyaWJ1dGVPdmVycmlkZSkge1xuICBhdHRyaWJ1dGVzID0gQXJyYXkuZnJvbShhdHRyaWJ1dGVzKTtcbiAgaWYgKGVsLl94X3ZpcnR1YWxEaXJlY3RpdmVzKSB7XG4gICAgbGV0IHZBdHRyaWJ1dGVzID0gT2JqZWN0LmVudHJpZXMoZWwuX3hfdmlydHVhbERpcmVjdGl2ZXMpLm1hcCgoW25hbWUsIHZhbHVlXSkgPT4gKHtuYW1lLCB2YWx1ZX0pKTtcbiAgICBsZXQgc3RhdGljQXR0cmlidXRlcyA9IGF0dHJpYnV0ZXNPbmx5KHZBdHRyaWJ1dGVzKTtcbiAgICB2QXR0cmlidXRlcyA9IHZBdHRyaWJ1dGVzLm1hcCgoYXR0cmlidXRlKSA9PiB7XG4gICAgICBpZiAoc3RhdGljQXR0cmlidXRlcy5maW5kKChhdHRyKSA9PiBhdHRyLm5hbWUgPT09IGF0dHJpYnV0ZS5uYW1lKSkge1xuICAgICAgICByZXR1cm4ge1xuICAgICAgICAgIG5hbWU6IGB4LWJpbmQ6JHthdHRyaWJ1dGUubmFtZX1gLFxuICAgICAgICAgIHZhbHVlOiBgXCIke2F0dHJpYnV0ZS52YWx1ZX1cImBcbiAgICAgICAgfTtcbiAgICAgIH1cbiAgICAgIHJldHVybiBhdHRyaWJ1dGU7XG4gICAgfSk7XG4gICAgYXR0cmlidXRlcyA9IGF0dHJpYnV0ZXMuY29uY2F0KHZBdHRyaWJ1dGVzKTtcbiAgfVxuICBsZXQgdHJhbnNmb3JtZWRBdHRyaWJ1dGVNYXAgPSB7fTtcbiAgbGV0IGRpcmVjdGl2ZXMyID0gYXR0cmlidXRlcy5tYXAodG9UcmFuc2Zvcm1lZEF0dHJpYnV0ZXMoKG5ld05hbWUsIG9sZE5hbWUpID0+IHRyYW5zZm9ybWVkQXR0cmlidXRlTWFwW25ld05hbWVdID0gb2xkTmFtZSkpLmZpbHRlcihvdXROb25BbHBpbmVBdHRyaWJ1dGVzKS5tYXAodG9QYXJzZWREaXJlY3RpdmVzKHRyYW5zZm9ybWVkQXR0cmlidXRlTWFwLCBvcmlnaW5hbEF0dHJpYnV0ZU92ZXJyaWRlKSkuc29ydChieVByaW9yaXR5KTtcbiAgcmV0dXJuIGRpcmVjdGl2ZXMyLm1hcCgoZGlyZWN0aXZlMikgPT4ge1xuICAgIHJldHVybiBnZXREaXJlY3RpdmVIYW5kbGVyKGVsLCBkaXJlY3RpdmUyKTtcbiAgfSk7XG59XG5mdW5jdGlvbiBhdHRyaWJ1dGVzT25seShhdHRyaWJ1dGVzKSB7XG4gIHJldHVybiBBcnJheS5mcm9tKGF0dHJpYnV0ZXMpLm1hcCh0b1RyYW5zZm9ybWVkQXR0cmlidXRlcygpKS5maWx0ZXIoKGF0dHIpID0+ICFvdXROb25BbHBpbmVBdHRyaWJ1dGVzKGF0dHIpKTtcbn1cbnZhciBpc0RlZmVycmluZ0hhbmRsZXJzID0gZmFsc2U7XG52YXIgZGlyZWN0aXZlSGFuZGxlclN0YWNrcyA9IG5ldyBNYXAoKTtcbnZhciBjdXJyZW50SGFuZGxlclN0YWNrS2V5ID0gU3ltYm9sKCk7XG5mdW5jdGlvbiBkZWZlckhhbmRsaW5nRGlyZWN0aXZlcyhjYWxsYmFjaykge1xuICBpc0RlZmVycmluZ0hhbmRsZXJzID0gdHJ1ZTtcbiAgbGV0IGtleSA9IFN5bWJvbCgpO1xuICBjdXJyZW50SGFuZGxlclN0YWNrS2V5ID0ga2V5O1xuICBkaXJlY3RpdmVIYW5kbGVyU3RhY2tzLnNldChrZXksIFtdKTtcbiAgbGV0IGZsdXNoSGFuZGxlcnMgPSAoKSA9PiB7XG4gICAgd2hpbGUgKGRpcmVjdGl2ZUhhbmRsZXJTdGFja3MuZ2V0KGtleSkubGVuZ3RoKVxuICAgICAgZGlyZWN0aXZlSGFuZGxlclN0YWNrcy5nZXQoa2V5KS5zaGlmdCgpKCk7XG4gICAgZGlyZWN0aXZlSGFuZGxlclN0YWNrcy5kZWxldGUoa2V5KTtcbiAgfTtcbiAgbGV0IHN0b3BEZWZlcnJpbmcgPSAoKSA9PiB7XG4gICAgaXNEZWZlcnJpbmdIYW5kbGVycyA9IGZhbHNlO1xuICAgIGZsdXNoSGFuZGxlcnMoKTtcbiAgfTtcbiAgY2FsbGJhY2soZmx1c2hIYW5kbGVycyk7XG4gIHN0b3BEZWZlcnJpbmcoKTtcbn1cbmZ1bmN0aW9uIGdldEVsZW1lbnRCb3VuZFV0aWxpdGllcyhlbCkge1xuICBsZXQgY2xlYW51cHMgPSBbXTtcbiAgbGV0IGNsZWFudXAyID0gKGNhbGxiYWNrKSA9PiBjbGVhbnVwcy5wdXNoKGNhbGxiYWNrKTtcbiAgbGV0IFtlZmZlY3QzLCBjbGVhbnVwRWZmZWN0XSA9IGVsZW1lbnRCb3VuZEVmZmVjdChlbCk7XG4gIGNsZWFudXBzLnB1c2goY2xlYW51cEVmZmVjdCk7XG4gIGxldCB1dGlsaXRpZXMgPSB7XG4gICAgQWxwaW5lOiBhbHBpbmVfZGVmYXVsdCxcbiAgICBlZmZlY3Q6IGVmZmVjdDMsXG4gICAgY2xlYW51cDogY2xlYW51cDIsXG4gICAgZXZhbHVhdGVMYXRlcjogZXZhbHVhdGVMYXRlci5iaW5kKGV2YWx1YXRlTGF0ZXIsIGVsKSxcbiAgICBldmFsdWF0ZTogZXZhbHVhdGUuYmluZChldmFsdWF0ZSwgZWwpXG4gIH07XG4gIGxldCBkb0NsZWFudXAgPSAoKSA9PiBjbGVhbnVwcy5mb3JFYWNoKChpKSA9PiBpKCkpO1xuICByZXR1cm4gW3V0aWxpdGllcywgZG9DbGVhbnVwXTtcbn1cbmZ1bmN0aW9uIGdldERpcmVjdGl2ZUhhbmRsZXIoZWwsIGRpcmVjdGl2ZTIpIHtcbiAgbGV0IG5vb3AgPSAoKSA9PiB7XG4gIH07XG4gIGxldCBoYW5kbGVyMyA9IGRpcmVjdGl2ZUhhbmRsZXJzW2RpcmVjdGl2ZTIudHlwZV0gfHwgbm9vcDtcbiAgbGV0IFt1dGlsaXRpZXMsIGNsZWFudXAyXSA9IGdldEVsZW1lbnRCb3VuZFV0aWxpdGllcyhlbCk7XG4gIG9uQXR0cmlidXRlUmVtb3ZlZChlbCwgZGlyZWN0aXZlMi5vcmlnaW5hbCwgY2xlYW51cDIpO1xuICBsZXQgZnVsbEhhbmRsZXIgPSAoKSA9PiB7XG4gICAgaWYgKGVsLl94X2lnbm9yZSB8fCBlbC5feF9pZ25vcmVTZWxmKVxuICAgICAgcmV0dXJuO1xuICAgIGhhbmRsZXIzLmlubGluZSAmJiBoYW5kbGVyMy5pbmxpbmUoZWwsIGRpcmVjdGl2ZTIsIHV0aWxpdGllcyk7XG4gICAgaGFuZGxlcjMgPSBoYW5kbGVyMy5iaW5kKGhhbmRsZXIzLCBlbCwgZGlyZWN0aXZlMiwgdXRpbGl0aWVzKTtcbiAgICBpc0RlZmVycmluZ0hhbmRsZXJzID8gZGlyZWN0aXZlSGFuZGxlclN0YWNrcy5nZXQoY3VycmVudEhhbmRsZXJTdGFja0tleSkucHVzaChoYW5kbGVyMykgOiBoYW5kbGVyMygpO1xuICB9O1xuICBmdWxsSGFuZGxlci5ydW5DbGVhbnVwcyA9IGNsZWFudXAyO1xuICByZXR1cm4gZnVsbEhhbmRsZXI7XG59XG52YXIgc3RhcnRpbmdXaXRoID0gKHN1YmplY3QsIHJlcGxhY2VtZW50KSA9PiAoe25hbWUsIHZhbHVlfSkgPT4ge1xuICBpZiAobmFtZS5zdGFydHNXaXRoKHN1YmplY3QpKVxuICAgIG5hbWUgPSBuYW1lLnJlcGxhY2Uoc3ViamVjdCwgcmVwbGFjZW1lbnQpO1xuICByZXR1cm4ge25hbWUsIHZhbHVlfTtcbn07XG52YXIgaW50byA9IChpKSA9PiBpO1xuZnVuY3Rpb24gdG9UcmFuc2Zvcm1lZEF0dHJpYnV0ZXMoY2FsbGJhY2sgPSAoKSA9PiB7XG59KSB7XG4gIHJldHVybiAoe25hbWUsIHZhbHVlfSkgPT4ge1xuICAgIGxldCB7bmFtZTogbmV3TmFtZSwgdmFsdWU6IG5ld1ZhbHVlfSA9IGF0dHJpYnV0ZVRyYW5zZm9ybWVycy5yZWR1Y2UoKGNhcnJ5LCB0cmFuc2Zvcm0pID0+IHtcbiAgICAgIHJldHVybiB0cmFuc2Zvcm0oY2FycnkpO1xuICAgIH0sIHtuYW1lLCB2YWx1ZX0pO1xuICAgIGlmIChuZXdOYW1lICE9PSBuYW1lKVxuICAgICAgY2FsbGJhY2sobmV3TmFtZSwgbmFtZSk7XG4gICAgcmV0dXJuIHtuYW1lOiBuZXdOYW1lLCB2YWx1ZTogbmV3VmFsdWV9O1xuICB9O1xufVxudmFyIGF0dHJpYnV0ZVRyYW5zZm9ybWVycyA9IFtdO1xuZnVuY3Rpb24gbWFwQXR0cmlidXRlcyhjYWxsYmFjaykge1xuICBhdHRyaWJ1dGVUcmFuc2Zvcm1lcnMucHVzaChjYWxsYmFjayk7XG59XG5mdW5jdGlvbiBvdXROb25BbHBpbmVBdHRyaWJ1dGVzKHtuYW1lfSkge1xuICByZXR1cm4gYWxwaW5lQXR0cmlidXRlUmVnZXgoKS50ZXN0KG5hbWUpO1xufVxudmFyIGFscGluZUF0dHJpYnV0ZVJlZ2V4ID0gKCkgPT4gbmV3IFJlZ0V4cChgXiR7cHJlZml4QXNTdHJpbmd9KFteOl4uXSspXFxcXGJgKTtcbmZ1bmN0aW9uIHRvUGFyc2VkRGlyZWN0aXZlcyh0cmFuc2Zvcm1lZEF0dHJpYnV0ZU1hcCwgb3JpZ2luYWxBdHRyaWJ1dGVPdmVycmlkZSkge1xuICByZXR1cm4gKHtuYW1lLCB2YWx1ZX0pID0+IHtcbiAgICBsZXQgdHlwZU1hdGNoID0gbmFtZS5tYXRjaChhbHBpbmVBdHRyaWJ1dGVSZWdleCgpKTtcbiAgICBsZXQgdmFsdWVNYXRjaCA9IG5hbWUubWF0Y2goLzooW2EtekEtWjAtOVxcLTpdKykvKTtcbiAgICBsZXQgbW9kaWZpZXJzID0gbmFtZS5tYXRjaCgvXFwuW14uXFxdXSsoPz1bXlxcXV0qJCkvZykgfHwgW107XG4gICAgbGV0IG9yaWdpbmFsID0gb3JpZ2luYWxBdHRyaWJ1dGVPdmVycmlkZSB8fCB0cmFuc2Zvcm1lZEF0dHJpYnV0ZU1hcFtuYW1lXSB8fCBuYW1lO1xuICAgIHJldHVybiB7XG4gICAgICB0eXBlOiB0eXBlTWF0Y2ggPyB0eXBlTWF0Y2hbMV0gOiBudWxsLFxuICAgICAgdmFsdWU6IHZhbHVlTWF0Y2ggPyB2YWx1ZU1hdGNoWzFdIDogbnVsbCxcbiAgICAgIG1vZGlmaWVyczogbW9kaWZpZXJzLm1hcCgoaSkgPT4gaS5yZXBsYWNlKFwiLlwiLCBcIlwiKSksXG4gICAgICBleHByZXNzaW9uOiB2YWx1ZSxcbiAgICAgIG9yaWdpbmFsXG4gICAgfTtcbiAgfTtcbn1cbnZhciBERUZBVUxUID0gXCJERUZBVUxUXCI7XG52YXIgZGlyZWN0aXZlT3JkZXIgPSBbXG4gIFwiaWdub3JlXCIsXG4gIFwicmVmXCIsXG4gIFwiZGF0YVwiLFxuICBcImlkXCIsXG4gIFwicmFkaW9cIixcbiAgXCJ0YWJzXCIsXG4gIFwic3dpdGNoXCIsXG4gIFwiZGlzY2xvc3VyZVwiLFxuICBcIm1lbnVcIixcbiAgXCJsaXN0Ym94XCIsXG4gIFwibGlzdFwiLFxuICBcIml0ZW1cIixcbiAgXCJjb21ib2JveFwiLFxuICBcImJpbmRcIixcbiAgXCJpbml0XCIsXG4gIFwiZm9yXCIsXG4gIFwibWFza1wiLFxuICBcIm1vZGVsXCIsXG4gIFwibW9kZWxhYmxlXCIsXG4gIFwidHJhbnNpdGlvblwiLFxuICBcInNob3dcIixcbiAgXCJpZlwiLFxuICBERUZBVUxULFxuICBcInRlbGVwb3J0XCJcbl07XG5mdW5jdGlvbiBieVByaW9yaXR5KGEsIGIpIHtcbiAgbGV0IHR5cGVBID0gZGlyZWN0aXZlT3JkZXIuaW5kZXhPZihhLnR5cGUpID09PSAtMSA/IERFRkFVTFQgOiBhLnR5cGU7XG4gIGxldCB0eXBlQiA9IGRpcmVjdGl2ZU9yZGVyLmluZGV4T2YoYi50eXBlKSA9PT0gLTEgPyBERUZBVUxUIDogYi50eXBlO1xuICByZXR1cm4gZGlyZWN0aXZlT3JkZXIuaW5kZXhPZih0eXBlQSkgLSBkaXJlY3RpdmVPcmRlci5pbmRleE9mKHR5cGVCKTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL3V0aWxzL2Rpc3BhdGNoLmpzXG5mdW5jdGlvbiBkaXNwYXRjaChlbCwgbmFtZSwgZGV0YWlsID0ge30pIHtcbiAgZWwuZGlzcGF0Y2hFdmVudChuZXcgQ3VzdG9tRXZlbnQobmFtZSwge1xuICAgIGRldGFpbCxcbiAgICBidWJibGVzOiB0cnVlLFxuICAgIGNvbXBvc2VkOiB0cnVlLFxuICAgIGNhbmNlbGFibGU6IHRydWVcbiAgfSkpO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvbmV4dFRpY2suanNcbnZhciB0aWNrU3RhY2sgPSBbXTtcbnZhciBpc0hvbGRpbmcgPSBmYWxzZTtcbmZ1bmN0aW9uIG5leHRUaWNrKGNhbGxiYWNrID0gKCkgPT4ge1xufSkge1xuICBxdWV1ZU1pY3JvdGFzaygoKSA9PiB7XG4gICAgaXNIb2xkaW5nIHx8IHNldFRpbWVvdXQoKCkgPT4ge1xuICAgICAgcmVsZWFzZU5leHRUaWNrcygpO1xuICAgIH0pO1xuICB9KTtcbiAgcmV0dXJuIG5ldyBQcm9taXNlKChyZXMpID0+IHtcbiAgICB0aWNrU3RhY2sucHVzaCgoKSA9PiB7XG4gICAgICBjYWxsYmFjaygpO1xuICAgICAgcmVzKCk7XG4gICAgfSk7XG4gIH0pO1xufVxuZnVuY3Rpb24gcmVsZWFzZU5leHRUaWNrcygpIHtcbiAgaXNIb2xkaW5nID0gZmFsc2U7XG4gIHdoaWxlICh0aWNrU3RhY2subGVuZ3RoKVxuICAgIHRpY2tTdGFjay5zaGlmdCgpKCk7XG59XG5mdW5jdGlvbiBob2xkTmV4dFRpY2tzKCkge1xuICBpc0hvbGRpbmcgPSB0cnVlO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvdXRpbHMvd2Fsay5qc1xuZnVuY3Rpb24gd2FsayhlbCwgY2FsbGJhY2spIHtcbiAgaWYgKHR5cGVvZiBTaGFkb3dSb290ID09PSBcImZ1bmN0aW9uXCIgJiYgZWwgaW5zdGFuY2VvZiBTaGFkb3dSb290KSB7XG4gICAgQXJyYXkuZnJvbShlbC5jaGlsZHJlbikuZm9yRWFjaCgoZWwyKSA9PiB3YWxrKGVsMiwgY2FsbGJhY2spKTtcbiAgICByZXR1cm47XG4gIH1cbiAgbGV0IHNraXAgPSBmYWxzZTtcbiAgY2FsbGJhY2soZWwsICgpID0+IHNraXAgPSB0cnVlKTtcbiAgaWYgKHNraXApXG4gICAgcmV0dXJuO1xuICBsZXQgbm9kZSA9IGVsLmZpcnN0RWxlbWVudENoaWxkO1xuICB3aGlsZSAobm9kZSkge1xuICAgIHdhbGsobm9kZSwgY2FsbGJhY2ssIGZhbHNlKTtcbiAgICBub2RlID0gbm9kZS5uZXh0RWxlbWVudFNpYmxpbmc7XG4gIH1cbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL3V0aWxzL3dhcm4uanNcbmZ1bmN0aW9uIHdhcm4obWVzc2FnZSwgLi4uYXJncykge1xuICBjb25zb2xlLndhcm4oYEFscGluZSBXYXJuaW5nOiAke21lc3NhZ2V9YCwgLi4uYXJncyk7XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9saWZlY3ljbGUuanNcbmZ1bmN0aW9uIHN0YXJ0KCkge1xuICBpZiAoIWRvY3VtZW50LmJvZHkpXG4gICAgd2FybihcIlVuYWJsZSB0byBpbml0aWFsaXplLiBUcnlpbmcgdG8gbG9hZCBBbHBpbmUgYmVmb3JlIGA8Ym9keT5gIGlzIGF2YWlsYWJsZS4gRGlkIHlvdSBmb3JnZXQgdG8gYWRkIGBkZWZlcmAgaW4gQWxwaW5lJ3MgYDxzY3JpcHQ+YCB0YWc/XCIpO1xuICBkaXNwYXRjaChkb2N1bWVudCwgXCJhbHBpbmU6aW5pdFwiKTtcbiAgZGlzcGF0Y2goZG9jdW1lbnQsIFwiYWxwaW5lOmluaXRpYWxpemluZ1wiKTtcbiAgc3RhcnRPYnNlcnZpbmdNdXRhdGlvbnMoKTtcbiAgb25FbEFkZGVkKChlbCkgPT4gaW5pdFRyZWUoZWwsIHdhbGspKTtcbiAgb25FbFJlbW92ZWQoKGVsKSA9PiBkZXN0cm95VHJlZShlbCkpO1xuICBvbkF0dHJpYnV0ZXNBZGRlZCgoZWwsIGF0dHJzKSA9PiB7XG4gICAgZGlyZWN0aXZlcyhlbCwgYXR0cnMpLmZvckVhY2goKGhhbmRsZSkgPT4gaGFuZGxlKCkpO1xuICB9KTtcbiAgbGV0IG91dE5lc3RlZENvbXBvbmVudHMgPSAoZWwpID0+ICFjbG9zZXN0Um9vdChlbC5wYXJlbnRFbGVtZW50LCB0cnVlKTtcbiAgQXJyYXkuZnJvbShkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKGFsbFNlbGVjdG9ycygpKSkuZmlsdGVyKG91dE5lc3RlZENvbXBvbmVudHMpLmZvckVhY2goKGVsKSA9PiB7XG4gICAgaW5pdFRyZWUoZWwpO1xuICB9KTtcbiAgZGlzcGF0Y2goZG9jdW1lbnQsIFwiYWxwaW5lOmluaXRpYWxpemVkXCIpO1xufVxudmFyIHJvb3RTZWxlY3RvckNhbGxiYWNrcyA9IFtdO1xudmFyIGluaXRTZWxlY3RvckNhbGxiYWNrcyA9IFtdO1xuZnVuY3Rpb24gcm9vdFNlbGVjdG9ycygpIHtcbiAgcmV0dXJuIHJvb3RTZWxlY3RvckNhbGxiYWNrcy5tYXAoKGZuKSA9PiBmbigpKTtcbn1cbmZ1bmN0aW9uIGFsbFNlbGVjdG9ycygpIHtcbiAgcmV0dXJuIHJvb3RTZWxlY3RvckNhbGxiYWNrcy5jb25jYXQoaW5pdFNlbGVjdG9yQ2FsbGJhY2tzKS5tYXAoKGZuKSA9PiBmbigpKTtcbn1cbmZ1bmN0aW9uIGFkZFJvb3RTZWxlY3RvcihzZWxlY3RvckNhbGxiYWNrKSB7XG4gIHJvb3RTZWxlY3RvckNhbGxiYWNrcy5wdXNoKHNlbGVjdG9yQ2FsbGJhY2spO1xufVxuZnVuY3Rpb24gYWRkSW5pdFNlbGVjdG9yKHNlbGVjdG9yQ2FsbGJhY2spIHtcbiAgaW5pdFNlbGVjdG9yQ2FsbGJhY2tzLnB1c2goc2VsZWN0b3JDYWxsYmFjayk7XG59XG5mdW5jdGlvbiBjbG9zZXN0Um9vdChlbCwgaW5jbHVkZUluaXRTZWxlY3RvcnMgPSBmYWxzZSkge1xuICByZXR1cm4gZmluZENsb3Nlc3QoZWwsIChlbGVtZW50KSA9PiB7XG4gICAgY29uc3Qgc2VsZWN0b3JzID0gaW5jbHVkZUluaXRTZWxlY3RvcnMgPyBhbGxTZWxlY3RvcnMoKSA6IHJvb3RTZWxlY3RvcnMoKTtcbiAgICBpZiAoc2VsZWN0b3JzLnNvbWUoKHNlbGVjdG9yKSA9PiBlbGVtZW50Lm1hdGNoZXMoc2VsZWN0b3IpKSlcbiAgICAgIHJldHVybiB0cnVlO1xuICB9KTtcbn1cbmZ1bmN0aW9uIGZpbmRDbG9zZXN0KGVsLCBjYWxsYmFjaykge1xuICBpZiAoIWVsKVxuICAgIHJldHVybjtcbiAgaWYgKGNhbGxiYWNrKGVsKSlcbiAgICByZXR1cm4gZWw7XG4gIGlmIChlbC5feF90ZWxlcG9ydEJhY2spXG4gICAgZWwgPSBlbC5feF90ZWxlcG9ydEJhY2s7XG4gIGlmICghZWwucGFyZW50RWxlbWVudClcbiAgICByZXR1cm47XG4gIHJldHVybiBmaW5kQ2xvc2VzdChlbC5wYXJlbnRFbGVtZW50LCBjYWxsYmFjayk7XG59XG5mdW5jdGlvbiBpc1Jvb3QoZWwpIHtcbiAgcmV0dXJuIHJvb3RTZWxlY3RvcnMoKS5zb21lKChzZWxlY3RvcikgPT4gZWwubWF0Y2hlcyhzZWxlY3RvcikpO1xufVxuZnVuY3Rpb24gaW5pdFRyZWUoZWwsIHdhbGtlciA9IHdhbGspIHtcbiAgZGVmZXJIYW5kbGluZ0RpcmVjdGl2ZXMoKCkgPT4ge1xuICAgIHdhbGtlcihlbCwgKGVsMiwgc2tpcCkgPT4ge1xuICAgICAgZGlyZWN0aXZlcyhlbDIsIGVsMi5hdHRyaWJ1dGVzKS5mb3JFYWNoKChoYW5kbGUpID0+IGhhbmRsZSgpKTtcbiAgICAgIGVsMi5feF9pZ25vcmUgJiYgc2tpcCgpO1xuICAgIH0pO1xuICB9KTtcbn1cbmZ1bmN0aW9uIGRlc3Ryb3lUcmVlKHJvb3QpIHtcbiAgd2Fsayhyb290LCAoZWwpID0+IGNsZWFudXBBdHRyaWJ1dGVzKGVsKSk7XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy91dGlscy9jbGFzc2VzLmpzXG5mdW5jdGlvbiBzZXRDbGFzc2VzKGVsLCB2YWx1ZSkge1xuICBpZiAoQXJyYXkuaXNBcnJheSh2YWx1ZSkpIHtcbiAgICByZXR1cm4gc2V0Q2xhc3Nlc0Zyb21TdHJpbmcoZWwsIHZhbHVlLmpvaW4oXCIgXCIpKTtcbiAgfSBlbHNlIGlmICh0eXBlb2YgdmFsdWUgPT09IFwib2JqZWN0XCIgJiYgdmFsdWUgIT09IG51bGwpIHtcbiAgICByZXR1cm4gc2V0Q2xhc3Nlc0Zyb21PYmplY3QoZWwsIHZhbHVlKTtcbiAgfSBlbHNlIGlmICh0eXBlb2YgdmFsdWUgPT09IFwiZnVuY3Rpb25cIikge1xuICAgIHJldHVybiBzZXRDbGFzc2VzKGVsLCB2YWx1ZSgpKTtcbiAgfVxuICByZXR1cm4gc2V0Q2xhc3Nlc0Zyb21TdHJpbmcoZWwsIHZhbHVlKTtcbn1cbmZ1bmN0aW9uIHNldENsYXNzZXNGcm9tU3RyaW5nKGVsLCBjbGFzc1N0cmluZykge1xuICBsZXQgc3BsaXQgPSAoY2xhc3NTdHJpbmcyKSA9PiBjbGFzc1N0cmluZzIuc3BsaXQoXCIgXCIpLmZpbHRlcihCb29sZWFuKTtcbiAgbGV0IG1pc3NpbmdDbGFzc2VzID0gKGNsYXNzU3RyaW5nMikgPT4gY2xhc3NTdHJpbmcyLnNwbGl0KFwiIFwiKS5maWx0ZXIoKGkpID0+ICFlbC5jbGFzc0xpc3QuY29udGFpbnMoaSkpLmZpbHRlcihCb29sZWFuKTtcbiAgbGV0IGFkZENsYXNzZXNBbmRSZXR1cm5VbmRvID0gKGNsYXNzZXMpID0+IHtcbiAgICBlbC5jbGFzc0xpc3QuYWRkKC4uLmNsYXNzZXMpO1xuICAgIHJldHVybiAoKSA9PiB7XG4gICAgICBlbC5jbGFzc0xpc3QucmVtb3ZlKC4uLmNsYXNzZXMpO1xuICAgIH07XG4gIH07XG4gIGNsYXNzU3RyaW5nID0gY2xhc3NTdHJpbmcgPT09IHRydWUgPyBjbGFzc1N0cmluZyA9IFwiXCIgOiBjbGFzc1N0cmluZyB8fCBcIlwiO1xuICByZXR1cm4gYWRkQ2xhc3Nlc0FuZFJldHVyblVuZG8obWlzc2luZ0NsYXNzZXMoY2xhc3NTdHJpbmcpKTtcbn1cbmZ1bmN0aW9uIHNldENsYXNzZXNGcm9tT2JqZWN0KGVsLCBjbGFzc09iamVjdCkge1xuICBsZXQgc3BsaXQgPSAoY2xhc3NTdHJpbmcpID0+IGNsYXNzU3RyaW5nLnNwbGl0KFwiIFwiKS5maWx0ZXIoQm9vbGVhbik7XG4gIGxldCBmb3JBZGQgPSBPYmplY3QuZW50cmllcyhjbGFzc09iamVjdCkuZmxhdE1hcCgoW2NsYXNzU3RyaW5nLCBib29sXSkgPT4gYm9vbCA/IHNwbGl0KGNsYXNzU3RyaW5nKSA6IGZhbHNlKS5maWx0ZXIoQm9vbGVhbik7XG4gIGxldCBmb3JSZW1vdmUgPSBPYmplY3QuZW50cmllcyhjbGFzc09iamVjdCkuZmxhdE1hcCgoW2NsYXNzU3RyaW5nLCBib29sXSkgPT4gIWJvb2wgPyBzcGxpdChjbGFzc1N0cmluZykgOiBmYWxzZSkuZmlsdGVyKEJvb2xlYW4pO1xuICBsZXQgYWRkZWQgPSBbXTtcbiAgbGV0IHJlbW92ZWQgPSBbXTtcbiAgZm9yUmVtb3ZlLmZvckVhY2goKGkpID0+IHtcbiAgICBpZiAoZWwuY2xhc3NMaXN0LmNvbnRhaW5zKGkpKSB7XG4gICAgICBlbC5jbGFzc0xpc3QucmVtb3ZlKGkpO1xuICAgICAgcmVtb3ZlZC5wdXNoKGkpO1xuICAgIH1cbiAgfSk7XG4gIGZvckFkZC5mb3JFYWNoKChpKSA9PiB7XG4gICAgaWYgKCFlbC5jbGFzc0xpc3QuY29udGFpbnMoaSkpIHtcbiAgICAgIGVsLmNsYXNzTGlzdC5hZGQoaSk7XG4gICAgICBhZGRlZC5wdXNoKGkpO1xuICAgIH1cbiAgfSk7XG4gIHJldHVybiAoKSA9PiB7XG4gICAgcmVtb3ZlZC5mb3JFYWNoKChpKSA9PiBlbC5jbGFzc0xpc3QuYWRkKGkpKTtcbiAgICBhZGRlZC5mb3JFYWNoKChpKSA9PiBlbC5jbGFzc0xpc3QucmVtb3ZlKGkpKTtcbiAgfTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL3V0aWxzL3N0eWxlcy5qc1xuZnVuY3Rpb24gc2V0U3R5bGVzKGVsLCB2YWx1ZSkge1xuICBpZiAodHlwZW9mIHZhbHVlID09PSBcIm9iamVjdFwiICYmIHZhbHVlICE9PSBudWxsKSB7XG4gICAgcmV0dXJuIHNldFN0eWxlc0Zyb21PYmplY3QoZWwsIHZhbHVlKTtcbiAgfVxuICByZXR1cm4gc2V0U3R5bGVzRnJvbVN0cmluZyhlbCwgdmFsdWUpO1xufVxuZnVuY3Rpb24gc2V0U3R5bGVzRnJvbU9iamVjdChlbCwgdmFsdWUpIHtcbiAgbGV0IHByZXZpb3VzU3R5bGVzID0ge307XG4gIE9iamVjdC5lbnRyaWVzKHZhbHVlKS5mb3JFYWNoKChba2V5LCB2YWx1ZTJdKSA9PiB7XG4gICAgcHJldmlvdXNTdHlsZXNba2V5XSA9IGVsLnN0eWxlW2tleV07XG4gICAgaWYgKCFrZXkuc3RhcnRzV2l0aChcIi0tXCIpKSB7XG4gICAgICBrZXkgPSBrZWJhYkNhc2Uoa2V5KTtcbiAgICB9XG4gICAgZWwuc3R5bGUuc2V0UHJvcGVydHkoa2V5LCB2YWx1ZTIpO1xuICB9KTtcbiAgc2V0VGltZW91dCgoKSA9PiB7XG4gICAgaWYgKGVsLnN0eWxlLmxlbmd0aCA9PT0gMCkge1xuICAgICAgZWwucmVtb3ZlQXR0cmlidXRlKFwic3R5bGVcIik7XG4gICAgfVxuICB9KTtcbiAgcmV0dXJuICgpID0+IHtcbiAgICBzZXRTdHlsZXMoZWwsIHByZXZpb3VzU3R5bGVzKTtcbiAgfTtcbn1cbmZ1bmN0aW9uIHNldFN0eWxlc0Zyb21TdHJpbmcoZWwsIHZhbHVlKSB7XG4gIGxldCBjYWNoZSA9IGVsLmdldEF0dHJpYnV0ZShcInN0eWxlXCIsIHZhbHVlKTtcbiAgZWwuc2V0QXR0cmlidXRlKFwic3R5bGVcIiwgdmFsdWUpO1xuICByZXR1cm4gKCkgPT4ge1xuICAgIGVsLnNldEF0dHJpYnV0ZShcInN0eWxlXCIsIGNhY2hlIHx8IFwiXCIpO1xuICB9O1xufVxuZnVuY3Rpb24ga2ViYWJDYXNlKHN1YmplY3QpIHtcbiAgcmV0dXJuIHN1YmplY3QucmVwbGFjZSgvKFthLXpdKShbQS1aXSkvZywgXCIkMS0kMlwiKS50b0xvd2VyQ2FzZSgpO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvdXRpbHMvb25jZS5qc1xuZnVuY3Rpb24gb25jZShjYWxsYmFjaywgZmFsbGJhY2sgPSAoKSA9PiB7XG59KSB7XG4gIGxldCBjYWxsZWQgPSBmYWxzZTtcbiAgcmV0dXJuIGZ1bmN0aW9uKCkge1xuICAgIGlmICghY2FsbGVkKSB7XG4gICAgICBjYWxsZWQgPSB0cnVlO1xuICAgICAgY2FsbGJhY2suYXBwbHkodGhpcywgYXJndW1lbnRzKTtcbiAgICB9IGVsc2Uge1xuICAgICAgZmFsbGJhY2suYXBwbHkodGhpcywgYXJndW1lbnRzKTtcbiAgICB9XG4gIH07XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9kaXJlY3RpdmVzL3gtdHJhbnNpdGlvbi5qc1xuZGlyZWN0aXZlKFwidHJhbnNpdGlvblwiLCAoZWwsIHt2YWx1ZSwgbW9kaWZpZXJzLCBleHByZXNzaW9ufSwge2V2YWx1YXRlOiBldmFsdWF0ZTJ9KSA9PiB7XG4gIGlmICh0eXBlb2YgZXhwcmVzc2lvbiA9PT0gXCJmdW5jdGlvblwiKVxuICAgIGV4cHJlc3Npb24gPSBldmFsdWF0ZTIoZXhwcmVzc2lvbik7XG4gIGlmICghZXhwcmVzc2lvbikge1xuICAgIHJlZ2lzdGVyVHJhbnNpdGlvbnNGcm9tSGVscGVyKGVsLCBtb2RpZmllcnMsIHZhbHVlKTtcbiAgfSBlbHNlIHtcbiAgICByZWdpc3RlclRyYW5zaXRpb25zRnJvbUNsYXNzU3RyaW5nKGVsLCBleHByZXNzaW9uLCB2YWx1ZSk7XG4gIH1cbn0pO1xuZnVuY3Rpb24gcmVnaXN0ZXJUcmFuc2l0aW9uc0Zyb21DbGFzc1N0cmluZyhlbCwgY2xhc3NTdHJpbmcsIHN0YWdlKSB7XG4gIHJlZ2lzdGVyVHJhbnNpdGlvbk9iamVjdChlbCwgc2V0Q2xhc3NlcywgXCJcIik7XG4gIGxldCBkaXJlY3RpdmVTdG9yYWdlTWFwID0ge1xuICAgIGVudGVyOiAoY2xhc3NlcykgPT4ge1xuICAgICAgZWwuX3hfdHJhbnNpdGlvbi5lbnRlci5kdXJpbmcgPSBjbGFzc2VzO1xuICAgIH0sXG4gICAgXCJlbnRlci1zdGFydFwiOiAoY2xhc3NlcykgPT4ge1xuICAgICAgZWwuX3hfdHJhbnNpdGlvbi5lbnRlci5zdGFydCA9IGNsYXNzZXM7XG4gICAgfSxcbiAgICBcImVudGVyLWVuZFwiOiAoY2xhc3NlcykgPT4ge1xuICAgICAgZWwuX3hfdHJhbnNpdGlvbi5lbnRlci5lbmQgPSBjbGFzc2VzO1xuICAgIH0sXG4gICAgbGVhdmU6IChjbGFzc2VzKSA9PiB7XG4gICAgICBlbC5feF90cmFuc2l0aW9uLmxlYXZlLmR1cmluZyA9IGNsYXNzZXM7XG4gICAgfSxcbiAgICBcImxlYXZlLXN0YXJ0XCI6IChjbGFzc2VzKSA9PiB7XG4gICAgICBlbC5feF90cmFuc2l0aW9uLmxlYXZlLnN0YXJ0ID0gY2xhc3NlcztcbiAgICB9LFxuICAgIFwibGVhdmUtZW5kXCI6IChjbGFzc2VzKSA9PiB7XG4gICAgICBlbC5feF90cmFuc2l0aW9uLmxlYXZlLmVuZCA9IGNsYXNzZXM7XG4gICAgfVxuICB9O1xuICBkaXJlY3RpdmVTdG9yYWdlTWFwW3N0YWdlXShjbGFzc1N0cmluZyk7XG59XG5mdW5jdGlvbiByZWdpc3RlclRyYW5zaXRpb25zRnJvbUhlbHBlcihlbCwgbW9kaWZpZXJzLCBzdGFnZSkge1xuICByZWdpc3RlclRyYW5zaXRpb25PYmplY3QoZWwsIHNldFN0eWxlcyk7XG4gIGxldCBkb2VzbnRTcGVjaWZ5ID0gIW1vZGlmaWVycy5pbmNsdWRlcyhcImluXCIpICYmICFtb2RpZmllcnMuaW5jbHVkZXMoXCJvdXRcIikgJiYgIXN0YWdlO1xuICBsZXQgdHJhbnNpdGlvbmluZ0luID0gZG9lc250U3BlY2lmeSB8fCBtb2RpZmllcnMuaW5jbHVkZXMoXCJpblwiKSB8fCBbXCJlbnRlclwiXS5pbmNsdWRlcyhzdGFnZSk7XG4gIGxldCB0cmFuc2l0aW9uaW5nT3V0ID0gZG9lc250U3BlY2lmeSB8fCBtb2RpZmllcnMuaW5jbHVkZXMoXCJvdXRcIikgfHwgW1wibGVhdmVcIl0uaW5jbHVkZXMoc3RhZ2UpO1xuICBpZiAobW9kaWZpZXJzLmluY2x1ZGVzKFwiaW5cIikgJiYgIWRvZXNudFNwZWNpZnkpIHtcbiAgICBtb2RpZmllcnMgPSBtb2RpZmllcnMuZmlsdGVyKChpLCBpbmRleCkgPT4gaW5kZXggPCBtb2RpZmllcnMuaW5kZXhPZihcIm91dFwiKSk7XG4gIH1cbiAgaWYgKG1vZGlmaWVycy5pbmNsdWRlcyhcIm91dFwiKSAmJiAhZG9lc250U3BlY2lmeSkge1xuICAgIG1vZGlmaWVycyA9IG1vZGlmaWVycy5maWx0ZXIoKGksIGluZGV4KSA9PiBpbmRleCA+IG1vZGlmaWVycy5pbmRleE9mKFwib3V0XCIpKTtcbiAgfVxuICBsZXQgd2FudHNBbGwgPSAhbW9kaWZpZXJzLmluY2x1ZGVzKFwib3BhY2l0eVwiKSAmJiAhbW9kaWZpZXJzLmluY2x1ZGVzKFwic2NhbGVcIik7XG4gIGxldCB3YW50c09wYWNpdHkgPSB3YW50c0FsbCB8fCBtb2RpZmllcnMuaW5jbHVkZXMoXCJvcGFjaXR5XCIpO1xuICBsZXQgd2FudHNTY2FsZSA9IHdhbnRzQWxsIHx8IG1vZGlmaWVycy5pbmNsdWRlcyhcInNjYWxlXCIpO1xuICBsZXQgb3BhY2l0eVZhbHVlID0gd2FudHNPcGFjaXR5ID8gMCA6IDE7XG4gIGxldCBzY2FsZVZhbHVlID0gd2FudHNTY2FsZSA/IG1vZGlmaWVyVmFsdWUobW9kaWZpZXJzLCBcInNjYWxlXCIsIDk1KSAvIDEwMCA6IDE7XG4gIGxldCBkZWxheSA9IG1vZGlmaWVyVmFsdWUobW9kaWZpZXJzLCBcImRlbGF5XCIsIDApO1xuICBsZXQgb3JpZ2luID0gbW9kaWZpZXJWYWx1ZShtb2RpZmllcnMsIFwib3JpZ2luXCIsIFwiY2VudGVyXCIpO1xuICBsZXQgcHJvcGVydHkgPSBcIm9wYWNpdHksIHRyYW5zZm9ybVwiO1xuICBsZXQgZHVyYXRpb25JbiA9IG1vZGlmaWVyVmFsdWUobW9kaWZpZXJzLCBcImR1cmF0aW9uXCIsIDE1MCkgLyAxZTM7XG4gIGxldCBkdXJhdGlvbk91dCA9IG1vZGlmaWVyVmFsdWUobW9kaWZpZXJzLCBcImR1cmF0aW9uXCIsIDc1KSAvIDFlMztcbiAgbGV0IGVhc2luZyA9IGBjdWJpYy1iZXppZXIoMC40LCAwLjAsIDAuMiwgMSlgO1xuICBpZiAodHJhbnNpdGlvbmluZ0luKSB7XG4gICAgZWwuX3hfdHJhbnNpdGlvbi5lbnRlci5kdXJpbmcgPSB7XG4gICAgICB0cmFuc2Zvcm1PcmlnaW46IG9yaWdpbixcbiAgICAgIHRyYW5zaXRpb25EZWxheTogZGVsYXksXG4gICAgICB0cmFuc2l0aW9uUHJvcGVydHk6IHByb3BlcnR5LFxuICAgICAgdHJhbnNpdGlvbkR1cmF0aW9uOiBgJHtkdXJhdGlvbklufXNgLFxuICAgICAgdHJhbnNpdGlvblRpbWluZ0Z1bmN0aW9uOiBlYXNpbmdcbiAgICB9O1xuICAgIGVsLl94X3RyYW5zaXRpb24uZW50ZXIuc3RhcnQgPSB7XG4gICAgICBvcGFjaXR5OiBvcGFjaXR5VmFsdWUsXG4gICAgICB0cmFuc2Zvcm06IGBzY2FsZSgke3NjYWxlVmFsdWV9KWBcbiAgICB9O1xuICAgIGVsLl94X3RyYW5zaXRpb24uZW50ZXIuZW5kID0ge1xuICAgICAgb3BhY2l0eTogMSxcbiAgICAgIHRyYW5zZm9ybTogYHNjYWxlKDEpYFxuICAgIH07XG4gIH1cbiAgaWYgKHRyYW5zaXRpb25pbmdPdXQpIHtcbiAgICBlbC5feF90cmFuc2l0aW9uLmxlYXZlLmR1cmluZyA9IHtcbiAgICAgIHRyYW5zZm9ybU9yaWdpbjogb3JpZ2luLFxuICAgICAgdHJhbnNpdGlvbkRlbGF5OiBkZWxheSxcbiAgICAgIHRyYW5zaXRpb25Qcm9wZXJ0eTogcHJvcGVydHksXG4gICAgICB0cmFuc2l0aW9uRHVyYXRpb246IGAke2R1cmF0aW9uT3V0fXNgLFxuICAgICAgdHJhbnNpdGlvblRpbWluZ0Z1bmN0aW9uOiBlYXNpbmdcbiAgICB9O1xuICAgIGVsLl94X3RyYW5zaXRpb24ubGVhdmUuc3RhcnQgPSB7XG4gICAgICBvcGFjaXR5OiAxLFxuICAgICAgdHJhbnNmb3JtOiBgc2NhbGUoMSlgXG4gICAgfTtcbiAgICBlbC5feF90cmFuc2l0aW9uLmxlYXZlLmVuZCA9IHtcbiAgICAgIG9wYWNpdHk6IG9wYWNpdHlWYWx1ZSxcbiAgICAgIHRyYW5zZm9ybTogYHNjYWxlKCR7c2NhbGVWYWx1ZX0pYFxuICAgIH07XG4gIH1cbn1cbmZ1bmN0aW9uIHJlZ2lzdGVyVHJhbnNpdGlvbk9iamVjdChlbCwgc2V0RnVuY3Rpb24sIGRlZmF1bHRWYWx1ZSA9IHt9KSB7XG4gIGlmICghZWwuX3hfdHJhbnNpdGlvbilcbiAgICBlbC5feF90cmFuc2l0aW9uID0ge1xuICAgICAgZW50ZXI6IHtkdXJpbmc6IGRlZmF1bHRWYWx1ZSwgc3RhcnQ6IGRlZmF1bHRWYWx1ZSwgZW5kOiBkZWZhdWx0VmFsdWV9LFxuICAgICAgbGVhdmU6IHtkdXJpbmc6IGRlZmF1bHRWYWx1ZSwgc3RhcnQ6IGRlZmF1bHRWYWx1ZSwgZW5kOiBkZWZhdWx0VmFsdWV9LFxuICAgICAgaW4oYmVmb3JlID0gKCkgPT4ge1xuICAgICAgfSwgYWZ0ZXIgPSAoKSA9PiB7XG4gICAgICB9KSB7XG4gICAgICAgIHRyYW5zaXRpb24oZWwsIHNldEZ1bmN0aW9uLCB7XG4gICAgICAgICAgZHVyaW5nOiB0aGlzLmVudGVyLmR1cmluZyxcbiAgICAgICAgICBzdGFydDogdGhpcy5lbnRlci5zdGFydCxcbiAgICAgICAgICBlbmQ6IHRoaXMuZW50ZXIuZW5kXG4gICAgICAgIH0sIGJlZm9yZSwgYWZ0ZXIpO1xuICAgICAgfSxcbiAgICAgIG91dChiZWZvcmUgPSAoKSA9PiB7XG4gICAgICB9LCBhZnRlciA9ICgpID0+IHtcbiAgICAgIH0pIHtcbiAgICAgICAgdHJhbnNpdGlvbihlbCwgc2V0RnVuY3Rpb24sIHtcbiAgICAgICAgICBkdXJpbmc6IHRoaXMubGVhdmUuZHVyaW5nLFxuICAgICAgICAgIHN0YXJ0OiB0aGlzLmxlYXZlLnN0YXJ0LFxuICAgICAgICAgIGVuZDogdGhpcy5sZWF2ZS5lbmRcbiAgICAgICAgfSwgYmVmb3JlLCBhZnRlcik7XG4gICAgICB9XG4gICAgfTtcbn1cbndpbmRvdy5FbGVtZW50LnByb3RvdHlwZS5feF90b2dnbGVBbmRDYXNjYWRlV2l0aFRyYW5zaXRpb25zID0gZnVuY3Rpb24oZWwsIHZhbHVlLCBzaG93LCBoaWRlKSB7XG4gIGNvbnN0IG5leHRUaWNrMiA9IGRvY3VtZW50LnZpc2liaWxpdHlTdGF0ZSA9PT0gXCJ2aXNpYmxlXCIgPyByZXF1ZXN0QW5pbWF0aW9uRnJhbWUgOiBzZXRUaW1lb3V0O1xuICBsZXQgY2xpY2tBd2F5Q29tcGF0aWJsZVNob3cgPSAoKSA9PiBuZXh0VGljazIoc2hvdyk7XG4gIGlmICh2YWx1ZSkge1xuICAgIGlmIChlbC5feF90cmFuc2l0aW9uICYmIChlbC5feF90cmFuc2l0aW9uLmVudGVyIHx8IGVsLl94X3RyYW5zaXRpb24ubGVhdmUpKSB7XG4gICAgICBlbC5feF90cmFuc2l0aW9uLmVudGVyICYmIChPYmplY3QuZW50cmllcyhlbC5feF90cmFuc2l0aW9uLmVudGVyLmR1cmluZykubGVuZ3RoIHx8IE9iamVjdC5lbnRyaWVzKGVsLl94X3RyYW5zaXRpb24uZW50ZXIuc3RhcnQpLmxlbmd0aCB8fCBPYmplY3QuZW50cmllcyhlbC5feF90cmFuc2l0aW9uLmVudGVyLmVuZCkubGVuZ3RoKSA/IGVsLl94X3RyYW5zaXRpb24uaW4oc2hvdykgOiBjbGlja0F3YXlDb21wYXRpYmxlU2hvdygpO1xuICAgIH0gZWxzZSB7XG4gICAgICBlbC5feF90cmFuc2l0aW9uID8gZWwuX3hfdHJhbnNpdGlvbi5pbihzaG93KSA6IGNsaWNrQXdheUNvbXBhdGlibGVTaG93KCk7XG4gICAgfVxuICAgIHJldHVybjtcbiAgfVxuICBlbC5feF9oaWRlUHJvbWlzZSA9IGVsLl94X3RyYW5zaXRpb24gPyBuZXcgUHJvbWlzZSgocmVzb2x2ZSwgcmVqZWN0KSA9PiB7XG4gICAgZWwuX3hfdHJhbnNpdGlvbi5vdXQoKCkgPT4ge1xuICAgIH0sICgpID0+IHJlc29sdmUoaGlkZSkpO1xuICAgIGVsLl94X3RyYW5zaXRpb25pbmcuYmVmb3JlQ2FuY2VsKCgpID0+IHJlamVjdCh7aXNGcm9tQ2FuY2VsbGVkVHJhbnNpdGlvbjogdHJ1ZX0pKTtcbiAgfSkgOiBQcm9taXNlLnJlc29sdmUoaGlkZSk7XG4gIHF1ZXVlTWljcm90YXNrKCgpID0+IHtcbiAgICBsZXQgY2xvc2VzdCA9IGNsb3Nlc3RIaWRlKGVsKTtcbiAgICBpZiAoY2xvc2VzdCkge1xuICAgICAgaWYgKCFjbG9zZXN0Ll94X2hpZGVDaGlsZHJlbilcbiAgICAgICAgY2xvc2VzdC5feF9oaWRlQ2hpbGRyZW4gPSBbXTtcbiAgICAgIGNsb3Nlc3QuX3hfaGlkZUNoaWxkcmVuLnB1c2goZWwpO1xuICAgIH0gZWxzZSB7XG4gICAgICBuZXh0VGljazIoKCkgPT4ge1xuICAgICAgICBsZXQgaGlkZUFmdGVyQ2hpbGRyZW4gPSAoZWwyKSA9PiB7XG4gICAgICAgICAgbGV0IGNhcnJ5ID0gUHJvbWlzZS5hbGwoW1xuICAgICAgICAgICAgZWwyLl94X2hpZGVQcm9taXNlLFxuICAgICAgICAgICAgLi4uKGVsMi5feF9oaWRlQ2hpbGRyZW4gfHwgW10pLm1hcChoaWRlQWZ0ZXJDaGlsZHJlbilcbiAgICAgICAgICBdKS50aGVuKChbaV0pID0+IGkoKSk7XG4gICAgICAgICAgZGVsZXRlIGVsMi5feF9oaWRlUHJvbWlzZTtcbiAgICAgICAgICBkZWxldGUgZWwyLl94X2hpZGVDaGlsZHJlbjtcbiAgICAgICAgICByZXR1cm4gY2Fycnk7XG4gICAgICAgIH07XG4gICAgICAgIGhpZGVBZnRlckNoaWxkcmVuKGVsKS5jYXRjaCgoZSkgPT4ge1xuICAgICAgICAgIGlmICghZS5pc0Zyb21DYW5jZWxsZWRUcmFuc2l0aW9uKVxuICAgICAgICAgICAgdGhyb3cgZTtcbiAgICAgICAgfSk7XG4gICAgICB9KTtcbiAgICB9XG4gIH0pO1xufTtcbmZ1bmN0aW9uIGNsb3Nlc3RIaWRlKGVsKSB7XG4gIGxldCBwYXJlbnQgPSBlbC5wYXJlbnROb2RlO1xuICBpZiAoIXBhcmVudClcbiAgICByZXR1cm47XG4gIHJldHVybiBwYXJlbnQuX3hfaGlkZVByb21pc2UgPyBwYXJlbnQgOiBjbG9zZXN0SGlkZShwYXJlbnQpO1xufVxuZnVuY3Rpb24gdHJhbnNpdGlvbihlbCwgc2V0RnVuY3Rpb24sIHtkdXJpbmcsIHN0YXJ0OiBzdGFydDIsIGVuZH0gPSB7fSwgYmVmb3JlID0gKCkgPT4ge1xufSwgYWZ0ZXIgPSAoKSA9PiB7XG59KSB7XG4gIGlmIChlbC5feF90cmFuc2l0aW9uaW5nKVxuICAgIGVsLl94X3RyYW5zaXRpb25pbmcuY2FuY2VsKCk7XG4gIGlmIChPYmplY3Qua2V5cyhkdXJpbmcpLmxlbmd0aCA9PT0gMCAmJiBPYmplY3Qua2V5cyhzdGFydDIpLmxlbmd0aCA9PT0gMCAmJiBPYmplY3Qua2V5cyhlbmQpLmxlbmd0aCA9PT0gMCkge1xuICAgIGJlZm9yZSgpO1xuICAgIGFmdGVyKCk7XG4gICAgcmV0dXJuO1xuICB9XG4gIGxldCB1bmRvU3RhcnQsIHVuZG9EdXJpbmcsIHVuZG9FbmQ7XG4gIHBlcmZvcm1UcmFuc2l0aW9uKGVsLCB7XG4gICAgc3RhcnQoKSB7XG4gICAgICB1bmRvU3RhcnQgPSBzZXRGdW5jdGlvbihlbCwgc3RhcnQyKTtcbiAgICB9LFxuICAgIGR1cmluZygpIHtcbiAgICAgIHVuZG9EdXJpbmcgPSBzZXRGdW5jdGlvbihlbCwgZHVyaW5nKTtcbiAgICB9LFxuICAgIGJlZm9yZSxcbiAgICBlbmQoKSB7XG4gICAgICB1bmRvU3RhcnQoKTtcbiAgICAgIHVuZG9FbmQgPSBzZXRGdW5jdGlvbihlbCwgZW5kKTtcbiAgICB9LFxuICAgIGFmdGVyLFxuICAgIGNsZWFudXAoKSB7XG4gICAgICB1bmRvRHVyaW5nKCk7XG4gICAgICB1bmRvRW5kKCk7XG4gICAgfVxuICB9KTtcbn1cbmZ1bmN0aW9uIHBlcmZvcm1UcmFuc2l0aW9uKGVsLCBzdGFnZXMpIHtcbiAgbGV0IGludGVycnVwdGVkLCByZWFjaGVkQmVmb3JlLCByZWFjaGVkRW5kO1xuICBsZXQgZmluaXNoID0gb25jZSgoKSA9PiB7XG4gICAgbXV0YXRlRG9tKCgpID0+IHtcbiAgICAgIGludGVycnVwdGVkID0gdHJ1ZTtcbiAgICAgIGlmICghcmVhY2hlZEJlZm9yZSlcbiAgICAgICAgc3RhZ2VzLmJlZm9yZSgpO1xuICAgICAgaWYgKCFyZWFjaGVkRW5kKSB7XG4gICAgICAgIHN0YWdlcy5lbmQoKTtcbiAgICAgICAgcmVsZWFzZU5leHRUaWNrcygpO1xuICAgICAgfVxuICAgICAgc3RhZ2VzLmFmdGVyKCk7XG4gICAgICBpZiAoZWwuaXNDb25uZWN0ZWQpXG4gICAgICAgIHN0YWdlcy5jbGVhbnVwKCk7XG4gICAgICBkZWxldGUgZWwuX3hfdHJhbnNpdGlvbmluZztcbiAgICB9KTtcbiAgfSk7XG4gIGVsLl94X3RyYW5zaXRpb25pbmcgPSB7XG4gICAgYmVmb3JlQ2FuY2VsczogW10sXG4gICAgYmVmb3JlQ2FuY2VsKGNhbGxiYWNrKSB7XG4gICAgICB0aGlzLmJlZm9yZUNhbmNlbHMucHVzaChjYWxsYmFjayk7XG4gICAgfSxcbiAgICBjYW5jZWw6IG9uY2UoZnVuY3Rpb24oKSB7XG4gICAgICB3aGlsZSAodGhpcy5iZWZvcmVDYW5jZWxzLmxlbmd0aCkge1xuICAgICAgICB0aGlzLmJlZm9yZUNhbmNlbHMuc2hpZnQoKSgpO1xuICAgICAgfVxuICAgICAgO1xuICAgICAgZmluaXNoKCk7XG4gICAgfSksXG4gICAgZmluaXNoXG4gIH07XG4gIG11dGF0ZURvbSgoKSA9PiB7XG4gICAgc3RhZ2VzLnN0YXJ0KCk7XG4gICAgc3RhZ2VzLmR1cmluZygpO1xuICB9KTtcbiAgaG9sZE5leHRUaWNrcygpO1xuICByZXF1ZXN0QW5pbWF0aW9uRnJhbWUoKCkgPT4ge1xuICAgIGlmIChpbnRlcnJ1cHRlZClcbiAgICAgIHJldHVybjtcbiAgICBsZXQgZHVyYXRpb24gPSBOdW1iZXIoZ2V0Q29tcHV0ZWRTdHlsZShlbCkudHJhbnNpdGlvbkR1cmF0aW9uLnJlcGxhY2UoLywuKi8sIFwiXCIpLnJlcGxhY2UoXCJzXCIsIFwiXCIpKSAqIDFlMztcbiAgICBsZXQgZGVsYXkgPSBOdW1iZXIoZ2V0Q29tcHV0ZWRTdHlsZShlbCkudHJhbnNpdGlvbkRlbGF5LnJlcGxhY2UoLywuKi8sIFwiXCIpLnJlcGxhY2UoXCJzXCIsIFwiXCIpKSAqIDFlMztcbiAgICBpZiAoZHVyYXRpb24gPT09IDApXG4gICAgICBkdXJhdGlvbiA9IE51bWJlcihnZXRDb21wdXRlZFN0eWxlKGVsKS5hbmltYXRpb25EdXJhdGlvbi5yZXBsYWNlKFwic1wiLCBcIlwiKSkgKiAxZTM7XG4gICAgbXV0YXRlRG9tKCgpID0+IHtcbiAgICAgIHN0YWdlcy5iZWZvcmUoKTtcbiAgICB9KTtcbiAgICByZWFjaGVkQmVmb3JlID0gdHJ1ZTtcbiAgICByZXF1ZXN0QW5pbWF0aW9uRnJhbWUoKCkgPT4ge1xuICAgICAgaWYgKGludGVycnVwdGVkKVxuICAgICAgICByZXR1cm47XG4gICAgICBtdXRhdGVEb20oKCkgPT4ge1xuICAgICAgICBzdGFnZXMuZW5kKCk7XG4gICAgICB9KTtcbiAgICAgIHJlbGVhc2VOZXh0VGlja3MoKTtcbiAgICAgIHNldFRpbWVvdXQoZWwuX3hfdHJhbnNpdGlvbmluZy5maW5pc2gsIGR1cmF0aW9uICsgZGVsYXkpO1xuICAgICAgcmVhY2hlZEVuZCA9IHRydWU7XG4gICAgfSk7XG4gIH0pO1xufVxuZnVuY3Rpb24gbW9kaWZpZXJWYWx1ZShtb2RpZmllcnMsIGtleSwgZmFsbGJhY2spIHtcbiAgaWYgKG1vZGlmaWVycy5pbmRleE9mKGtleSkgPT09IC0xKVxuICAgIHJldHVybiBmYWxsYmFjaztcbiAgY29uc3QgcmF3VmFsdWUgPSBtb2RpZmllcnNbbW9kaWZpZXJzLmluZGV4T2Yoa2V5KSArIDFdO1xuICBpZiAoIXJhd1ZhbHVlKVxuICAgIHJldHVybiBmYWxsYmFjaztcbiAgaWYgKGtleSA9PT0gXCJzY2FsZVwiKSB7XG4gICAgaWYgKGlzTmFOKHJhd1ZhbHVlKSlcbiAgICAgIHJldHVybiBmYWxsYmFjaztcbiAgfVxuICBpZiAoa2V5ID09PSBcImR1cmF0aW9uXCIpIHtcbiAgICBsZXQgbWF0Y2ggPSByYXdWYWx1ZS5tYXRjaCgvKFswLTldKyltcy8pO1xuICAgIGlmIChtYXRjaClcbiAgICAgIHJldHVybiBtYXRjaFsxXTtcbiAgfVxuICBpZiAoa2V5ID09PSBcIm9yaWdpblwiKSB7XG4gICAgaWYgKFtcInRvcFwiLCBcInJpZ2h0XCIsIFwibGVmdFwiLCBcImNlbnRlclwiLCBcImJvdHRvbVwiXS5pbmNsdWRlcyhtb2RpZmllcnNbbW9kaWZpZXJzLmluZGV4T2Yoa2V5KSArIDJdKSkge1xuICAgICAgcmV0dXJuIFtyYXdWYWx1ZSwgbW9kaWZpZXJzW21vZGlmaWVycy5pbmRleE9mKGtleSkgKyAyXV0uam9pbihcIiBcIik7XG4gICAgfVxuICB9XG4gIHJldHVybiByYXdWYWx1ZTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL2Nsb25lLmpzXG52YXIgaXNDbG9uaW5nID0gZmFsc2U7XG5mdW5jdGlvbiBza2lwRHVyaW5nQ2xvbmUoY2FsbGJhY2ssIGZhbGxiYWNrID0gKCkgPT4ge1xufSkge1xuICByZXR1cm4gKC4uLmFyZ3MpID0+IGlzQ2xvbmluZyA/IGZhbGxiYWNrKC4uLmFyZ3MpIDogY2FsbGJhY2soLi4uYXJncyk7XG59XG5mdW5jdGlvbiBjbG9uZShvbGRFbCwgbmV3RWwpIHtcbiAgaWYgKCFuZXdFbC5feF9kYXRhU3RhY2spXG4gICAgbmV3RWwuX3hfZGF0YVN0YWNrID0gb2xkRWwuX3hfZGF0YVN0YWNrO1xuICBpc0Nsb25pbmcgPSB0cnVlO1xuICBkb250UmVnaXN0ZXJSZWFjdGl2ZVNpZGVFZmZlY3RzKCgpID0+IHtcbiAgICBjbG9uZVRyZWUobmV3RWwpO1xuICB9KTtcbiAgaXNDbG9uaW5nID0gZmFsc2U7XG59XG5mdW5jdGlvbiBjbG9uZVRyZWUoZWwpIHtcbiAgbGV0IGhhc1J1blRocm91Z2hGaXJzdEVsID0gZmFsc2U7XG4gIGxldCBzaGFsbG93V2Fsa2VyID0gKGVsMiwgY2FsbGJhY2spID0+IHtcbiAgICB3YWxrKGVsMiwgKGVsMywgc2tpcCkgPT4ge1xuICAgICAgaWYgKGhhc1J1blRocm91Z2hGaXJzdEVsICYmIGlzUm9vdChlbDMpKVxuICAgICAgICByZXR1cm4gc2tpcCgpO1xuICAgICAgaGFzUnVuVGhyb3VnaEZpcnN0RWwgPSB0cnVlO1xuICAgICAgY2FsbGJhY2soZWwzLCBza2lwKTtcbiAgICB9KTtcbiAgfTtcbiAgaW5pdFRyZWUoZWwsIHNoYWxsb3dXYWxrZXIpO1xufVxuZnVuY3Rpb24gZG9udFJlZ2lzdGVyUmVhY3RpdmVTaWRlRWZmZWN0cyhjYWxsYmFjaykge1xuICBsZXQgY2FjaGUgPSBlZmZlY3Q7XG4gIG92ZXJyaWRlRWZmZWN0KChjYWxsYmFjazIsIGVsKSA9PiB7XG4gICAgbGV0IHN0b3JlZEVmZmVjdCA9IGNhY2hlKGNhbGxiYWNrMik7XG4gICAgcmVsZWFzZShzdG9yZWRFZmZlY3QpO1xuICAgIHJldHVybiAoKSA9PiB7XG4gICAgfTtcbiAgfSk7XG4gIGNhbGxiYWNrKCk7XG4gIG92ZXJyaWRlRWZmZWN0KGNhY2hlKTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL3V0aWxzL2JpbmQuanNcbmZ1bmN0aW9uIGJpbmQoZWwsIG5hbWUsIHZhbHVlLCBtb2RpZmllcnMgPSBbXSkge1xuICBpZiAoIWVsLl94X2JpbmRpbmdzKVxuICAgIGVsLl94X2JpbmRpbmdzID0gcmVhY3RpdmUoe30pO1xuICBlbC5feF9iaW5kaW5nc1tuYW1lXSA9IHZhbHVlO1xuICBuYW1lID0gbW9kaWZpZXJzLmluY2x1ZGVzKFwiY2FtZWxcIikgPyBjYW1lbENhc2UobmFtZSkgOiBuYW1lO1xuICBzd2l0Y2ggKG5hbWUpIHtcbiAgICBjYXNlIFwidmFsdWVcIjpcbiAgICAgIGJpbmRJbnB1dFZhbHVlKGVsLCB2YWx1ZSk7XG4gICAgICBicmVhaztcbiAgICBjYXNlIFwic3R5bGVcIjpcbiAgICAgIGJpbmRTdHlsZXMoZWwsIHZhbHVlKTtcbiAgICAgIGJyZWFrO1xuICAgIGNhc2UgXCJjbGFzc1wiOlxuICAgICAgYmluZENsYXNzZXMoZWwsIHZhbHVlKTtcbiAgICAgIGJyZWFrO1xuICAgIGRlZmF1bHQ6XG4gICAgICBiaW5kQXR0cmlidXRlKGVsLCBuYW1lLCB2YWx1ZSk7XG4gICAgICBicmVhaztcbiAgfVxufVxuZnVuY3Rpb24gYmluZElucHV0VmFsdWUoZWwsIHZhbHVlKSB7XG4gIGlmIChlbC50eXBlID09PSBcInJhZGlvXCIpIHtcbiAgICBpZiAoZWwuYXR0cmlidXRlcy52YWx1ZSA9PT0gdm9pZCAwKSB7XG4gICAgICBlbC52YWx1ZSA9IHZhbHVlO1xuICAgIH1cbiAgICBpZiAod2luZG93LmZyb21Nb2RlbCkge1xuICAgICAgZWwuY2hlY2tlZCA9IGNoZWNrZWRBdHRyTG9vc2VDb21wYXJlKGVsLnZhbHVlLCB2YWx1ZSk7XG4gICAgfVxuICB9IGVsc2UgaWYgKGVsLnR5cGUgPT09IFwiY2hlY2tib3hcIikge1xuICAgIGlmIChOdW1iZXIuaXNJbnRlZ2VyKHZhbHVlKSkge1xuICAgICAgZWwudmFsdWUgPSB2YWx1ZTtcbiAgICB9IGVsc2UgaWYgKCFOdW1iZXIuaXNJbnRlZ2VyKHZhbHVlKSAmJiAhQXJyYXkuaXNBcnJheSh2YWx1ZSkgJiYgdHlwZW9mIHZhbHVlICE9PSBcImJvb2xlYW5cIiAmJiAhW251bGwsIHZvaWQgMF0uaW5jbHVkZXModmFsdWUpKSB7XG4gICAgICBlbC52YWx1ZSA9IFN0cmluZyh2YWx1ZSk7XG4gICAgfSBlbHNlIHtcbiAgICAgIGlmIChBcnJheS5pc0FycmF5KHZhbHVlKSkge1xuICAgICAgICBlbC5jaGVja2VkID0gdmFsdWUuc29tZSgodmFsKSA9PiBjaGVja2VkQXR0ckxvb3NlQ29tcGFyZSh2YWwsIGVsLnZhbHVlKSk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICBlbC5jaGVja2VkID0gISF2YWx1ZTtcbiAgICAgIH1cbiAgICB9XG4gIH0gZWxzZSBpZiAoZWwudGFnTmFtZSA9PT0gXCJTRUxFQ1RcIikge1xuICAgIHVwZGF0ZVNlbGVjdChlbCwgdmFsdWUpO1xuICB9IGVsc2Uge1xuICAgIGlmIChlbC52YWx1ZSA9PT0gdmFsdWUpXG4gICAgICByZXR1cm47XG4gICAgZWwudmFsdWUgPSB2YWx1ZTtcbiAgfVxufVxuZnVuY3Rpb24gYmluZENsYXNzZXMoZWwsIHZhbHVlKSB7XG4gIGlmIChlbC5feF91bmRvQWRkZWRDbGFzc2VzKVxuICAgIGVsLl94X3VuZG9BZGRlZENsYXNzZXMoKTtcbiAgZWwuX3hfdW5kb0FkZGVkQ2xhc3NlcyA9IHNldENsYXNzZXMoZWwsIHZhbHVlKTtcbn1cbmZ1bmN0aW9uIGJpbmRTdHlsZXMoZWwsIHZhbHVlKSB7XG4gIGlmIChlbC5feF91bmRvQWRkZWRTdHlsZXMpXG4gICAgZWwuX3hfdW5kb0FkZGVkU3R5bGVzKCk7XG4gIGVsLl94X3VuZG9BZGRlZFN0eWxlcyA9IHNldFN0eWxlcyhlbCwgdmFsdWUpO1xufVxuZnVuY3Rpb24gYmluZEF0dHJpYnV0ZShlbCwgbmFtZSwgdmFsdWUpIHtcbiAgaWYgKFtudWxsLCB2b2lkIDAsIGZhbHNlXS5pbmNsdWRlcyh2YWx1ZSkgJiYgYXR0cmlidXRlU2hvdWxkbnRCZVByZXNlcnZlZElmRmFsc3kobmFtZSkpIHtcbiAgICBlbC5yZW1vdmVBdHRyaWJ1dGUobmFtZSk7XG4gIH0gZWxzZSB7XG4gICAgaWYgKGlzQm9vbGVhbkF0dHIobmFtZSkpXG4gICAgICB2YWx1ZSA9IG5hbWU7XG4gICAgc2V0SWZDaGFuZ2VkKGVsLCBuYW1lLCB2YWx1ZSk7XG4gIH1cbn1cbmZ1bmN0aW9uIHNldElmQ2hhbmdlZChlbCwgYXR0ck5hbWUsIHZhbHVlKSB7XG4gIGlmIChlbC5nZXRBdHRyaWJ1dGUoYXR0ck5hbWUpICE9IHZhbHVlKSB7XG4gICAgZWwuc2V0QXR0cmlidXRlKGF0dHJOYW1lLCB2YWx1ZSk7XG4gIH1cbn1cbmZ1bmN0aW9uIHVwZGF0ZVNlbGVjdChlbCwgdmFsdWUpIHtcbiAgY29uc3QgYXJyYXlXcmFwcGVkVmFsdWUgPSBbXS5jb25jYXQodmFsdWUpLm1hcCgodmFsdWUyKSA9PiB7XG4gICAgcmV0dXJuIHZhbHVlMiArIFwiXCI7XG4gIH0pO1xuICBBcnJheS5mcm9tKGVsLm9wdGlvbnMpLmZvckVhY2goKG9wdGlvbikgPT4ge1xuICAgIG9wdGlvbi5zZWxlY3RlZCA9IGFycmF5V3JhcHBlZFZhbHVlLmluY2x1ZGVzKG9wdGlvbi52YWx1ZSk7XG4gIH0pO1xufVxuZnVuY3Rpb24gY2FtZWxDYXNlKHN1YmplY3QpIHtcbiAgcmV0dXJuIHN1YmplY3QudG9Mb3dlckNhc2UoKS5yZXBsYWNlKC8tKFxcdykvZywgKG1hdGNoLCBjaGFyKSA9PiBjaGFyLnRvVXBwZXJDYXNlKCkpO1xufVxuZnVuY3Rpb24gY2hlY2tlZEF0dHJMb29zZUNvbXBhcmUodmFsdWVBLCB2YWx1ZUIpIHtcbiAgcmV0dXJuIHZhbHVlQSA9PSB2YWx1ZUI7XG59XG5mdW5jdGlvbiBpc0Jvb2xlYW5BdHRyKGF0dHJOYW1lKSB7XG4gIGNvbnN0IGJvb2xlYW5BdHRyaWJ1dGVzID0gW1xuICAgIFwiZGlzYWJsZWRcIixcbiAgICBcImNoZWNrZWRcIixcbiAgICBcInJlcXVpcmVkXCIsXG4gICAgXCJyZWFkb25seVwiLFxuICAgIFwiaGlkZGVuXCIsXG4gICAgXCJvcGVuXCIsXG4gICAgXCJzZWxlY3RlZFwiLFxuICAgIFwiYXV0b2ZvY3VzXCIsXG4gICAgXCJpdGVtc2NvcGVcIixcbiAgICBcIm11bHRpcGxlXCIsXG4gICAgXCJub3ZhbGlkYXRlXCIsXG4gICAgXCJhbGxvd2Z1bGxzY3JlZW5cIixcbiAgICBcImFsbG93cGF5bWVudHJlcXVlc3RcIixcbiAgICBcImZvcm1ub3ZhbGlkYXRlXCIsXG4gICAgXCJhdXRvcGxheVwiLFxuICAgIFwiY29udHJvbHNcIixcbiAgICBcImxvb3BcIixcbiAgICBcIm11dGVkXCIsXG4gICAgXCJwbGF5c2lubGluZVwiLFxuICAgIFwiZGVmYXVsdFwiLFxuICAgIFwiaXNtYXBcIixcbiAgICBcInJldmVyc2VkXCIsXG4gICAgXCJhc3luY1wiLFxuICAgIFwiZGVmZXJcIixcbiAgICBcIm5vbW9kdWxlXCJcbiAgXTtcbiAgcmV0dXJuIGJvb2xlYW5BdHRyaWJ1dGVzLmluY2x1ZGVzKGF0dHJOYW1lKTtcbn1cbmZ1bmN0aW9uIGF0dHJpYnV0ZVNob3VsZG50QmVQcmVzZXJ2ZWRJZkZhbHN5KG5hbWUpIHtcbiAgcmV0dXJuICFbXCJhcmlhLXByZXNzZWRcIiwgXCJhcmlhLWNoZWNrZWRcIiwgXCJhcmlhLWV4cGFuZGVkXCIsIFwiYXJpYS1zZWxlY3RlZFwiXS5pbmNsdWRlcyhuYW1lKTtcbn1cbmZ1bmN0aW9uIGdldEJpbmRpbmcoZWwsIG5hbWUsIGZhbGxiYWNrKSB7XG4gIGlmIChlbC5feF9iaW5kaW5ncyAmJiBlbC5feF9iaW5kaW5nc1tuYW1lXSAhPT0gdm9pZCAwKVxuICAgIHJldHVybiBlbC5feF9iaW5kaW5nc1tuYW1lXTtcbiAgbGV0IGF0dHIgPSBlbC5nZXRBdHRyaWJ1dGUobmFtZSk7XG4gIGlmIChhdHRyID09PSBudWxsKVxuICAgIHJldHVybiB0eXBlb2YgZmFsbGJhY2sgPT09IFwiZnVuY3Rpb25cIiA/IGZhbGxiYWNrKCkgOiBmYWxsYmFjaztcbiAgaWYgKGF0dHIgPT09IFwiXCIpXG4gICAgcmV0dXJuIHRydWU7XG4gIGlmIChpc0Jvb2xlYW5BdHRyKG5hbWUpKSB7XG4gICAgcmV0dXJuICEhW25hbWUsIFwidHJ1ZVwiXS5pbmNsdWRlcyhhdHRyKTtcbiAgfVxuICByZXR1cm4gYXR0cjtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL3V0aWxzL2RlYm91bmNlLmpzXG5mdW5jdGlvbiBkZWJvdW5jZShmdW5jLCB3YWl0KSB7XG4gIHZhciB0aW1lb3V0O1xuICByZXR1cm4gZnVuY3Rpb24oKSB7XG4gICAgdmFyIGNvbnRleHQgPSB0aGlzLCBhcmdzID0gYXJndW1lbnRzO1xuICAgIHZhciBsYXRlciA9IGZ1bmN0aW9uKCkge1xuICAgICAgdGltZW91dCA9IG51bGw7XG4gICAgICBmdW5jLmFwcGx5KGNvbnRleHQsIGFyZ3MpO1xuICAgIH07XG4gICAgY2xlYXJUaW1lb3V0KHRpbWVvdXQpO1xuICAgIHRpbWVvdXQgPSBzZXRUaW1lb3V0KGxhdGVyLCB3YWl0KTtcbiAgfTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL3V0aWxzL3Rocm90dGxlLmpzXG5mdW5jdGlvbiB0aHJvdHRsZShmdW5jLCBsaW1pdCkge1xuICBsZXQgaW5UaHJvdHRsZTtcbiAgcmV0dXJuIGZ1bmN0aW9uKCkge1xuICAgIGxldCBjb250ZXh0ID0gdGhpcywgYXJncyA9IGFyZ3VtZW50cztcbiAgICBpZiAoIWluVGhyb3R0bGUpIHtcbiAgICAgIGZ1bmMuYXBwbHkoY29udGV4dCwgYXJncyk7XG4gICAgICBpblRocm90dGxlID0gdHJ1ZTtcbiAgICAgIHNldFRpbWVvdXQoKCkgPT4gaW5UaHJvdHRsZSA9IGZhbHNlLCBsaW1pdCk7XG4gICAgfVxuICB9O1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvcGx1Z2luLmpzXG5mdW5jdGlvbiBwbHVnaW4oY2FsbGJhY2spIHtcbiAgY2FsbGJhY2soYWxwaW5lX2RlZmF1bHQpO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvc3RvcmUuanNcbnZhciBzdG9yZXMgPSB7fTtcbnZhciBpc1JlYWN0aXZlID0gZmFsc2U7XG5mdW5jdGlvbiBzdG9yZShuYW1lLCB2YWx1ZSkge1xuICBpZiAoIWlzUmVhY3RpdmUpIHtcbiAgICBzdG9yZXMgPSByZWFjdGl2ZShzdG9yZXMpO1xuICAgIGlzUmVhY3RpdmUgPSB0cnVlO1xuICB9XG4gIGlmICh2YWx1ZSA9PT0gdm9pZCAwKSB7XG4gICAgcmV0dXJuIHN0b3Jlc1tuYW1lXTtcbiAgfVxuICBzdG9yZXNbbmFtZV0gPSB2YWx1ZTtcbiAgaWYgKHR5cGVvZiB2YWx1ZSA9PT0gXCJvYmplY3RcIiAmJiB2YWx1ZSAhPT0gbnVsbCAmJiB2YWx1ZS5oYXNPd25Qcm9wZXJ0eShcImluaXRcIikgJiYgdHlwZW9mIHZhbHVlLmluaXQgPT09IFwiZnVuY3Rpb25cIikge1xuICAgIHN0b3Jlc1tuYW1lXS5pbml0KCk7XG4gIH1cbiAgaW5pdEludGVyY2VwdG9ycyhzdG9yZXNbbmFtZV0pO1xufVxuZnVuY3Rpb24gZ2V0U3RvcmVzKCkge1xuICByZXR1cm4gc3RvcmVzO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvYmluZHMuanNcbnZhciBiaW5kcyA9IHt9O1xuZnVuY3Rpb24gYmluZDIobmFtZSwgYmluZGluZ3MpIHtcbiAgbGV0IGdldEJpbmRpbmdzID0gdHlwZW9mIGJpbmRpbmdzICE9PSBcImZ1bmN0aW9uXCIgPyAoKSA9PiBiaW5kaW5ncyA6IGJpbmRpbmdzO1xuICBpZiAobmFtZSBpbnN0YW5jZW9mIEVsZW1lbnQpIHtcbiAgICBhcHBseUJpbmRpbmdzT2JqZWN0KG5hbWUsIGdldEJpbmRpbmdzKCkpO1xuICB9IGVsc2Uge1xuICAgIGJpbmRzW25hbWVdID0gZ2V0QmluZGluZ3M7XG4gIH1cbn1cbmZ1bmN0aW9uIGluamVjdEJpbmRpbmdQcm92aWRlcnMob2JqKSB7XG4gIE9iamVjdC5lbnRyaWVzKGJpbmRzKS5mb3JFYWNoKChbbmFtZSwgY2FsbGJhY2tdKSA9PiB7XG4gICAgT2JqZWN0LmRlZmluZVByb3BlcnR5KG9iaiwgbmFtZSwge1xuICAgICAgZ2V0KCkge1xuICAgICAgICByZXR1cm4gKC4uLmFyZ3MpID0+IHtcbiAgICAgICAgICByZXR1cm4gY2FsbGJhY2soLi4uYXJncyk7XG4gICAgICAgIH07XG4gICAgICB9XG4gICAgfSk7XG4gIH0pO1xuICByZXR1cm4gb2JqO1xufVxuZnVuY3Rpb24gYXBwbHlCaW5kaW5nc09iamVjdChlbCwgb2JqLCBvcmlnaW5hbCkge1xuICBsZXQgY2xlYW51cFJ1bm5lcnMgPSBbXTtcbiAgd2hpbGUgKGNsZWFudXBSdW5uZXJzLmxlbmd0aClcbiAgICBjbGVhbnVwUnVubmVycy5wb3AoKSgpO1xuICBsZXQgYXR0cmlidXRlcyA9IE9iamVjdC5lbnRyaWVzKG9iaikubWFwKChbbmFtZSwgdmFsdWVdKSA9PiAoe25hbWUsIHZhbHVlfSkpO1xuICBsZXQgc3RhdGljQXR0cmlidXRlcyA9IGF0dHJpYnV0ZXNPbmx5KGF0dHJpYnV0ZXMpO1xuICBhdHRyaWJ1dGVzID0gYXR0cmlidXRlcy5tYXAoKGF0dHJpYnV0ZSkgPT4ge1xuICAgIGlmIChzdGF0aWNBdHRyaWJ1dGVzLmZpbmQoKGF0dHIpID0+IGF0dHIubmFtZSA9PT0gYXR0cmlidXRlLm5hbWUpKSB7XG4gICAgICByZXR1cm4ge1xuICAgICAgICBuYW1lOiBgeC1iaW5kOiR7YXR0cmlidXRlLm5hbWV9YCxcbiAgICAgICAgdmFsdWU6IGBcIiR7YXR0cmlidXRlLnZhbHVlfVwiYFxuICAgICAgfTtcbiAgICB9XG4gICAgcmV0dXJuIGF0dHJpYnV0ZTtcbiAgfSk7XG4gIGRpcmVjdGl2ZXMoZWwsIGF0dHJpYnV0ZXMsIG9yaWdpbmFsKS5tYXAoKGhhbmRsZSkgPT4ge1xuICAgIGNsZWFudXBSdW5uZXJzLnB1c2goaGFuZGxlLnJ1bkNsZWFudXBzKTtcbiAgICBoYW5kbGUoKTtcbiAgfSk7XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9kYXRhcy5qc1xudmFyIGRhdGFzID0ge307XG5mdW5jdGlvbiBkYXRhKG5hbWUsIGNhbGxiYWNrKSB7XG4gIGRhdGFzW25hbWVdID0gY2FsbGJhY2s7XG59XG5mdW5jdGlvbiBpbmplY3REYXRhUHJvdmlkZXJzKG9iaiwgY29udGV4dCkge1xuICBPYmplY3QuZW50cmllcyhkYXRhcykuZm9yRWFjaCgoW25hbWUsIGNhbGxiYWNrXSkgPT4ge1xuICAgIE9iamVjdC5kZWZpbmVQcm9wZXJ0eShvYmosIG5hbWUsIHtcbiAgICAgIGdldCgpIHtcbiAgICAgICAgcmV0dXJuICguLi5hcmdzKSA9PiB7XG4gICAgICAgICAgcmV0dXJuIGNhbGxiYWNrLmJpbmQoY29udGV4dCkoLi4uYXJncyk7XG4gICAgICAgIH07XG4gICAgICB9LFxuICAgICAgZW51bWVyYWJsZTogZmFsc2VcbiAgICB9KTtcbiAgfSk7XG4gIHJldHVybiBvYmo7XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9hbHBpbmUuanNcbnZhciBBbHBpbmUgPSB7XG4gIGdldCByZWFjdGl2ZSgpIHtcbiAgICByZXR1cm4gcmVhY3RpdmU7XG4gIH0sXG4gIGdldCByZWxlYXNlKCkge1xuICAgIHJldHVybiByZWxlYXNlO1xuICB9LFxuICBnZXQgZWZmZWN0KCkge1xuICAgIHJldHVybiBlZmZlY3Q7XG4gIH0sXG4gIGdldCByYXcoKSB7XG4gICAgcmV0dXJuIHJhdztcbiAgfSxcbiAgdmVyc2lvbjogXCIzLjEwLjVcIixcbiAgZmx1c2hBbmRTdG9wRGVmZXJyaW5nTXV0YXRpb25zLFxuICBkb250QXV0b0V2YWx1YXRlRnVuY3Rpb25zLFxuICBkaXNhYmxlRWZmZWN0U2NoZWR1bGluZyxcbiAgc2V0UmVhY3Rpdml0eUVuZ2luZSxcbiAgY2xvc2VzdERhdGFTdGFjayxcbiAgc2tpcER1cmluZ0Nsb25lLFxuICBhZGRSb290U2VsZWN0b3IsXG4gIGFkZEluaXRTZWxlY3RvcixcbiAgYWRkU2NvcGVUb05vZGUsXG4gIGRlZmVyTXV0YXRpb25zLFxuICBtYXBBdHRyaWJ1dGVzLFxuICBldmFsdWF0ZUxhdGVyLFxuICBzZXRFdmFsdWF0b3IsXG4gIG1lcmdlUHJveGllcyxcbiAgZmluZENsb3Nlc3QsXG4gIGNsb3Nlc3RSb290LFxuICBpbnRlcmNlcHRvcixcbiAgdHJhbnNpdGlvbixcbiAgc2V0U3R5bGVzLFxuICBtdXRhdGVEb20sXG4gIGRpcmVjdGl2ZSxcbiAgdGhyb3R0bGUsXG4gIGRlYm91bmNlLFxuICBldmFsdWF0ZSxcbiAgaW5pdFRyZWUsXG4gIG5leHRUaWNrLFxuICBwcmVmaXhlZDogcHJlZml4LFxuICBwcmVmaXg6IHNldFByZWZpeCxcbiAgcGx1Z2luLFxuICBtYWdpYyxcbiAgc3RvcmUsXG4gIHN0YXJ0LFxuICBjbG9uZSxcbiAgYm91bmQ6IGdldEJpbmRpbmcsXG4gICRkYXRhOiBzY29wZSxcbiAgZGF0YSxcbiAgYmluZDogYmluZDJcbn07XG52YXIgYWxwaW5lX2RlZmF1bHQgPSBBbHBpbmU7XG5cbi8vIG5vZGVfbW9kdWxlcy9AdnVlL3NoYXJlZC9kaXN0L3NoYXJlZC5lc20tYnVuZGxlci5qc1xuZnVuY3Rpb24gbWFrZU1hcChzdHIsIGV4cGVjdHNMb3dlckNhc2UpIHtcbiAgY29uc3QgbWFwID0gT2JqZWN0LmNyZWF0ZShudWxsKTtcbiAgY29uc3QgbGlzdCA9IHN0ci5zcGxpdChcIixcIik7XG4gIGZvciAobGV0IGkgPSAwOyBpIDwgbGlzdC5sZW5ndGg7IGkrKykge1xuICAgIG1hcFtsaXN0W2ldXSA9IHRydWU7XG4gIH1cbiAgcmV0dXJuIGV4cGVjdHNMb3dlckNhc2UgPyAodmFsKSA9PiAhIW1hcFt2YWwudG9Mb3dlckNhc2UoKV0gOiAodmFsKSA9PiAhIW1hcFt2YWxdO1xufVxudmFyIFBhdGNoRmxhZ05hbWVzID0ge1xuICBbMV06IGBURVhUYCxcbiAgWzJdOiBgQ0xBU1NgLFxuICBbNF06IGBTVFlMRWAsXG4gIFs4XTogYFBST1BTYCxcbiAgWzE2XTogYEZVTExfUFJPUFNgLFxuICBbMzJdOiBgSFlEUkFURV9FVkVOVFNgLFxuICBbNjRdOiBgU1RBQkxFX0ZSQUdNRU5UYCxcbiAgWzEyOF06IGBLRVlFRF9GUkFHTUVOVGAsXG4gIFsyNTZdOiBgVU5LRVlFRF9GUkFHTUVOVGAsXG4gIFs1MTJdOiBgTkVFRF9QQVRDSGAsXG4gIFsxMDI0XTogYERZTkFNSUNfU0xPVFNgLFxuICBbMjA0OF06IGBERVZfUk9PVF9GUkFHTUVOVGAsXG4gIFstMV06IGBIT0lTVEVEYCxcbiAgWy0yXTogYEJBSUxgXG59O1xudmFyIHNsb3RGbGFnc1RleHQgPSB7XG4gIFsxXTogXCJTVEFCTEVcIixcbiAgWzJdOiBcIkRZTkFNSUNcIixcbiAgWzNdOiBcIkZPUldBUkRFRFwiXG59O1xudmFyIHNwZWNpYWxCb29sZWFuQXR0cnMgPSBgaXRlbXNjb3BlLGFsbG93ZnVsbHNjcmVlbixmb3Jtbm92YWxpZGF0ZSxpc21hcCxub21vZHVsZSxub3ZhbGlkYXRlLHJlYWRvbmx5YDtcbnZhciBpc0Jvb2xlYW5BdHRyMiA9IC8qIEBfX1BVUkVfXyAqLyBtYWtlTWFwKHNwZWNpYWxCb29sZWFuQXR0cnMgKyBgLGFzeW5jLGF1dG9mb2N1cyxhdXRvcGxheSxjb250cm9scyxkZWZhdWx0LGRlZmVyLGRpc2FibGVkLGhpZGRlbixsb29wLG9wZW4scmVxdWlyZWQscmV2ZXJzZWQsc2NvcGVkLHNlYW1sZXNzLGNoZWNrZWQsbXV0ZWQsbXVsdGlwbGUsc2VsZWN0ZWRgKTtcbnZhciBFTVBUWV9PQkogPSB0cnVlID8gT2JqZWN0LmZyZWV6ZSh7fSkgOiB7fTtcbnZhciBFTVBUWV9BUlIgPSB0cnVlID8gT2JqZWN0LmZyZWV6ZShbXSkgOiBbXTtcbnZhciBleHRlbmQgPSBPYmplY3QuYXNzaWduO1xudmFyIGhhc093blByb3BlcnR5ID0gT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eTtcbnZhciBoYXNPd24gPSAodmFsLCBrZXkpID0+IGhhc093blByb3BlcnR5LmNhbGwodmFsLCBrZXkpO1xudmFyIGlzQXJyYXkgPSBBcnJheS5pc0FycmF5O1xudmFyIGlzTWFwID0gKHZhbCkgPT4gdG9UeXBlU3RyaW5nKHZhbCkgPT09IFwiW29iamVjdCBNYXBdXCI7XG52YXIgaXNTdHJpbmcgPSAodmFsKSA9PiB0eXBlb2YgdmFsID09PSBcInN0cmluZ1wiO1xudmFyIGlzU3ltYm9sID0gKHZhbCkgPT4gdHlwZW9mIHZhbCA9PT0gXCJzeW1ib2xcIjtcbnZhciBpc09iamVjdCA9ICh2YWwpID0+IHZhbCAhPT0gbnVsbCAmJiB0eXBlb2YgdmFsID09PSBcIm9iamVjdFwiO1xudmFyIG9iamVjdFRvU3RyaW5nID0gT2JqZWN0LnByb3RvdHlwZS50b1N0cmluZztcbnZhciB0b1R5cGVTdHJpbmcgPSAodmFsdWUpID0+IG9iamVjdFRvU3RyaW5nLmNhbGwodmFsdWUpO1xudmFyIHRvUmF3VHlwZSA9ICh2YWx1ZSkgPT4ge1xuICByZXR1cm4gdG9UeXBlU3RyaW5nKHZhbHVlKS5zbGljZSg4LCAtMSk7XG59O1xudmFyIGlzSW50ZWdlcktleSA9IChrZXkpID0+IGlzU3RyaW5nKGtleSkgJiYga2V5ICE9PSBcIk5hTlwiICYmIGtleVswXSAhPT0gXCItXCIgJiYgXCJcIiArIHBhcnNlSW50KGtleSwgMTApID09PSBrZXk7XG52YXIgY2FjaGVTdHJpbmdGdW5jdGlvbiA9IChmbikgPT4ge1xuICBjb25zdCBjYWNoZSA9IE9iamVjdC5jcmVhdGUobnVsbCk7XG4gIHJldHVybiAoc3RyKSA9PiB7XG4gICAgY29uc3QgaGl0ID0gY2FjaGVbc3RyXTtcbiAgICByZXR1cm4gaGl0IHx8IChjYWNoZVtzdHJdID0gZm4oc3RyKSk7XG4gIH07XG59O1xudmFyIGNhbWVsaXplUkUgPSAvLShcXHcpL2c7XG52YXIgY2FtZWxpemUgPSBjYWNoZVN0cmluZ0Z1bmN0aW9uKChzdHIpID0+IHtcbiAgcmV0dXJuIHN0ci5yZXBsYWNlKGNhbWVsaXplUkUsIChfLCBjKSA9PiBjID8gYy50b1VwcGVyQ2FzZSgpIDogXCJcIik7XG59KTtcbnZhciBoeXBoZW5hdGVSRSA9IC9cXEIoW0EtWl0pL2c7XG52YXIgaHlwaGVuYXRlID0gY2FjaGVTdHJpbmdGdW5jdGlvbigoc3RyKSA9PiBzdHIucmVwbGFjZShoeXBoZW5hdGVSRSwgXCItJDFcIikudG9Mb3dlckNhc2UoKSk7XG52YXIgY2FwaXRhbGl6ZSA9IGNhY2hlU3RyaW5nRnVuY3Rpb24oKHN0cikgPT4gc3RyLmNoYXJBdCgwKS50b1VwcGVyQ2FzZSgpICsgc3RyLnNsaWNlKDEpKTtcbnZhciB0b0hhbmRsZXJLZXkgPSBjYWNoZVN0cmluZ0Z1bmN0aW9uKChzdHIpID0+IHN0ciA/IGBvbiR7Y2FwaXRhbGl6ZShzdHIpfWAgOiBgYCk7XG52YXIgaGFzQ2hhbmdlZCA9ICh2YWx1ZSwgb2xkVmFsdWUpID0+IHZhbHVlICE9PSBvbGRWYWx1ZSAmJiAodmFsdWUgPT09IHZhbHVlIHx8IG9sZFZhbHVlID09PSBvbGRWYWx1ZSk7XG5cbi8vIG5vZGVfbW9kdWxlcy9AdnVlL3JlYWN0aXZpdHkvZGlzdC9yZWFjdGl2aXR5LmVzbS1idW5kbGVyLmpzXG52YXIgdGFyZ2V0TWFwID0gbmV3IFdlYWtNYXAoKTtcbnZhciBlZmZlY3RTdGFjayA9IFtdO1xudmFyIGFjdGl2ZUVmZmVjdDtcbnZhciBJVEVSQVRFX0tFWSA9IFN5bWJvbCh0cnVlID8gXCJpdGVyYXRlXCIgOiBcIlwiKTtcbnZhciBNQVBfS0VZX0lURVJBVEVfS0VZID0gU3ltYm9sKHRydWUgPyBcIk1hcCBrZXkgaXRlcmF0ZVwiIDogXCJcIik7XG5mdW5jdGlvbiBpc0VmZmVjdChmbikge1xuICByZXR1cm4gZm4gJiYgZm4uX2lzRWZmZWN0ID09PSB0cnVlO1xufVxuZnVuY3Rpb24gZWZmZWN0Mihmbiwgb3B0aW9ucyA9IEVNUFRZX09CSikge1xuICBpZiAoaXNFZmZlY3QoZm4pKSB7XG4gICAgZm4gPSBmbi5yYXc7XG4gIH1cbiAgY29uc3QgZWZmZWN0MyA9IGNyZWF0ZVJlYWN0aXZlRWZmZWN0KGZuLCBvcHRpb25zKTtcbiAgaWYgKCFvcHRpb25zLmxhenkpIHtcbiAgICBlZmZlY3QzKCk7XG4gIH1cbiAgcmV0dXJuIGVmZmVjdDM7XG59XG5mdW5jdGlvbiBzdG9wKGVmZmVjdDMpIHtcbiAgaWYgKGVmZmVjdDMuYWN0aXZlKSB7XG4gICAgY2xlYW51cChlZmZlY3QzKTtcbiAgICBpZiAoZWZmZWN0My5vcHRpb25zLm9uU3RvcCkge1xuICAgICAgZWZmZWN0My5vcHRpb25zLm9uU3RvcCgpO1xuICAgIH1cbiAgICBlZmZlY3QzLmFjdGl2ZSA9IGZhbHNlO1xuICB9XG59XG52YXIgdWlkID0gMDtcbmZ1bmN0aW9uIGNyZWF0ZVJlYWN0aXZlRWZmZWN0KGZuLCBvcHRpb25zKSB7XG4gIGNvbnN0IGVmZmVjdDMgPSBmdW5jdGlvbiByZWFjdGl2ZUVmZmVjdCgpIHtcbiAgICBpZiAoIWVmZmVjdDMuYWN0aXZlKSB7XG4gICAgICByZXR1cm4gZm4oKTtcbiAgICB9XG4gICAgaWYgKCFlZmZlY3RTdGFjay5pbmNsdWRlcyhlZmZlY3QzKSkge1xuICAgICAgY2xlYW51cChlZmZlY3QzKTtcbiAgICAgIHRyeSB7XG4gICAgICAgIGVuYWJsZVRyYWNraW5nKCk7XG4gICAgICAgIGVmZmVjdFN0YWNrLnB1c2goZWZmZWN0Myk7XG4gICAgICAgIGFjdGl2ZUVmZmVjdCA9IGVmZmVjdDM7XG4gICAgICAgIHJldHVybiBmbigpO1xuICAgICAgfSBmaW5hbGx5IHtcbiAgICAgICAgZWZmZWN0U3RhY2sucG9wKCk7XG4gICAgICAgIHJlc2V0VHJhY2tpbmcoKTtcbiAgICAgICAgYWN0aXZlRWZmZWN0ID0gZWZmZWN0U3RhY2tbZWZmZWN0U3RhY2subGVuZ3RoIC0gMV07XG4gICAgICB9XG4gICAgfVxuICB9O1xuICBlZmZlY3QzLmlkID0gdWlkKys7XG4gIGVmZmVjdDMuYWxsb3dSZWN1cnNlID0gISFvcHRpb25zLmFsbG93UmVjdXJzZTtcbiAgZWZmZWN0My5faXNFZmZlY3QgPSB0cnVlO1xuICBlZmZlY3QzLmFjdGl2ZSA9IHRydWU7XG4gIGVmZmVjdDMucmF3ID0gZm47XG4gIGVmZmVjdDMuZGVwcyA9IFtdO1xuICBlZmZlY3QzLm9wdGlvbnMgPSBvcHRpb25zO1xuICByZXR1cm4gZWZmZWN0Mztcbn1cbmZ1bmN0aW9uIGNsZWFudXAoZWZmZWN0Mykge1xuICBjb25zdCB7ZGVwc30gPSBlZmZlY3QzO1xuICBpZiAoZGVwcy5sZW5ndGgpIHtcbiAgICBmb3IgKGxldCBpID0gMDsgaSA8IGRlcHMubGVuZ3RoOyBpKyspIHtcbiAgICAgIGRlcHNbaV0uZGVsZXRlKGVmZmVjdDMpO1xuICAgIH1cbiAgICBkZXBzLmxlbmd0aCA9IDA7XG4gIH1cbn1cbnZhciBzaG91bGRUcmFjayA9IHRydWU7XG52YXIgdHJhY2tTdGFjayA9IFtdO1xuZnVuY3Rpb24gcGF1c2VUcmFja2luZygpIHtcbiAgdHJhY2tTdGFjay5wdXNoKHNob3VsZFRyYWNrKTtcbiAgc2hvdWxkVHJhY2sgPSBmYWxzZTtcbn1cbmZ1bmN0aW9uIGVuYWJsZVRyYWNraW5nKCkge1xuICB0cmFja1N0YWNrLnB1c2goc2hvdWxkVHJhY2spO1xuICBzaG91bGRUcmFjayA9IHRydWU7XG59XG5mdW5jdGlvbiByZXNldFRyYWNraW5nKCkge1xuICBjb25zdCBsYXN0ID0gdHJhY2tTdGFjay5wb3AoKTtcbiAgc2hvdWxkVHJhY2sgPSBsYXN0ID09PSB2b2lkIDAgPyB0cnVlIDogbGFzdDtcbn1cbmZ1bmN0aW9uIHRyYWNrKHRhcmdldCwgdHlwZSwga2V5KSB7XG4gIGlmICghc2hvdWxkVHJhY2sgfHwgYWN0aXZlRWZmZWN0ID09PSB2b2lkIDApIHtcbiAgICByZXR1cm47XG4gIH1cbiAgbGV0IGRlcHNNYXAgPSB0YXJnZXRNYXAuZ2V0KHRhcmdldCk7XG4gIGlmICghZGVwc01hcCkge1xuICAgIHRhcmdldE1hcC5zZXQodGFyZ2V0LCBkZXBzTWFwID0gbmV3IE1hcCgpKTtcbiAgfVxuICBsZXQgZGVwID0gZGVwc01hcC5nZXQoa2V5KTtcbiAgaWYgKCFkZXApIHtcbiAgICBkZXBzTWFwLnNldChrZXksIGRlcCA9IG5ldyBTZXQoKSk7XG4gIH1cbiAgaWYgKCFkZXAuaGFzKGFjdGl2ZUVmZmVjdCkpIHtcbiAgICBkZXAuYWRkKGFjdGl2ZUVmZmVjdCk7XG4gICAgYWN0aXZlRWZmZWN0LmRlcHMucHVzaChkZXApO1xuICAgIGlmIChhY3RpdmVFZmZlY3Qub3B0aW9ucy5vblRyYWNrKSB7XG4gICAgICBhY3RpdmVFZmZlY3Qub3B0aW9ucy5vblRyYWNrKHtcbiAgICAgICAgZWZmZWN0OiBhY3RpdmVFZmZlY3QsXG4gICAgICAgIHRhcmdldCxcbiAgICAgICAgdHlwZSxcbiAgICAgICAga2V5XG4gICAgICB9KTtcbiAgICB9XG4gIH1cbn1cbmZ1bmN0aW9uIHRyaWdnZXIodGFyZ2V0LCB0eXBlLCBrZXksIG5ld1ZhbHVlLCBvbGRWYWx1ZSwgb2xkVGFyZ2V0KSB7XG4gIGNvbnN0IGRlcHNNYXAgPSB0YXJnZXRNYXAuZ2V0KHRhcmdldCk7XG4gIGlmICghZGVwc01hcCkge1xuICAgIHJldHVybjtcbiAgfVxuICBjb25zdCBlZmZlY3RzID0gbmV3IFNldCgpO1xuICBjb25zdCBhZGQyID0gKGVmZmVjdHNUb0FkZCkgPT4ge1xuICAgIGlmIChlZmZlY3RzVG9BZGQpIHtcbiAgICAgIGVmZmVjdHNUb0FkZC5mb3JFYWNoKChlZmZlY3QzKSA9PiB7XG4gICAgICAgIGlmIChlZmZlY3QzICE9PSBhY3RpdmVFZmZlY3QgfHwgZWZmZWN0My5hbGxvd1JlY3Vyc2UpIHtcbiAgICAgICAgICBlZmZlY3RzLmFkZChlZmZlY3QzKTtcbiAgICAgICAgfVxuICAgICAgfSk7XG4gICAgfVxuICB9O1xuICBpZiAodHlwZSA9PT0gXCJjbGVhclwiKSB7XG4gICAgZGVwc01hcC5mb3JFYWNoKGFkZDIpO1xuICB9IGVsc2UgaWYgKGtleSA9PT0gXCJsZW5ndGhcIiAmJiBpc0FycmF5KHRhcmdldCkpIHtcbiAgICBkZXBzTWFwLmZvckVhY2goKGRlcCwga2V5MikgPT4ge1xuICAgICAgaWYgKGtleTIgPT09IFwibGVuZ3RoXCIgfHwga2V5MiA+PSBuZXdWYWx1ZSkge1xuICAgICAgICBhZGQyKGRlcCk7XG4gICAgICB9XG4gICAgfSk7XG4gIH0gZWxzZSB7XG4gICAgaWYgKGtleSAhPT0gdm9pZCAwKSB7XG4gICAgICBhZGQyKGRlcHNNYXAuZ2V0KGtleSkpO1xuICAgIH1cbiAgICBzd2l0Y2ggKHR5cGUpIHtcbiAgICAgIGNhc2UgXCJhZGRcIjpcbiAgICAgICAgaWYgKCFpc0FycmF5KHRhcmdldCkpIHtcbiAgICAgICAgICBhZGQyKGRlcHNNYXAuZ2V0KElURVJBVEVfS0VZKSk7XG4gICAgICAgICAgaWYgKGlzTWFwKHRhcmdldCkpIHtcbiAgICAgICAgICAgIGFkZDIoZGVwc01hcC5nZXQoTUFQX0tFWV9JVEVSQVRFX0tFWSkpO1xuICAgICAgICAgIH1cbiAgICAgICAgfSBlbHNlIGlmIChpc0ludGVnZXJLZXkoa2V5KSkge1xuICAgICAgICAgIGFkZDIoZGVwc01hcC5nZXQoXCJsZW5ndGhcIikpO1xuICAgICAgICB9XG4gICAgICAgIGJyZWFrO1xuICAgICAgY2FzZSBcImRlbGV0ZVwiOlxuICAgICAgICBpZiAoIWlzQXJyYXkodGFyZ2V0KSkge1xuICAgICAgICAgIGFkZDIoZGVwc01hcC5nZXQoSVRFUkFURV9LRVkpKTtcbiAgICAgICAgICBpZiAoaXNNYXAodGFyZ2V0KSkge1xuICAgICAgICAgICAgYWRkMihkZXBzTWFwLmdldChNQVBfS0VZX0lURVJBVEVfS0VZKSk7XG4gICAgICAgICAgfVxuICAgICAgICB9XG4gICAgICAgIGJyZWFrO1xuICAgICAgY2FzZSBcInNldFwiOlxuICAgICAgICBpZiAoaXNNYXAodGFyZ2V0KSkge1xuICAgICAgICAgIGFkZDIoZGVwc01hcC5nZXQoSVRFUkFURV9LRVkpKTtcbiAgICAgICAgfVxuICAgICAgICBicmVhaztcbiAgICB9XG4gIH1cbiAgY29uc3QgcnVuID0gKGVmZmVjdDMpID0+IHtcbiAgICBpZiAoZWZmZWN0My5vcHRpb25zLm9uVHJpZ2dlcikge1xuICAgICAgZWZmZWN0My5vcHRpb25zLm9uVHJpZ2dlcih7XG4gICAgICAgIGVmZmVjdDogZWZmZWN0MyxcbiAgICAgICAgdGFyZ2V0LFxuICAgICAgICBrZXksXG4gICAgICAgIHR5cGUsXG4gICAgICAgIG5ld1ZhbHVlLFxuICAgICAgICBvbGRWYWx1ZSxcbiAgICAgICAgb2xkVGFyZ2V0XG4gICAgICB9KTtcbiAgICB9XG4gICAgaWYgKGVmZmVjdDMub3B0aW9ucy5zY2hlZHVsZXIpIHtcbiAgICAgIGVmZmVjdDMub3B0aW9ucy5zY2hlZHVsZXIoZWZmZWN0Myk7XG4gICAgfSBlbHNlIHtcbiAgICAgIGVmZmVjdDMoKTtcbiAgICB9XG4gIH07XG4gIGVmZmVjdHMuZm9yRWFjaChydW4pO1xufVxudmFyIGlzTm9uVHJhY2thYmxlS2V5cyA9IC8qIEBfX1BVUkVfXyAqLyBtYWtlTWFwKGBfX3Byb3RvX18sX192X2lzUmVmLF9faXNWdWVgKTtcbnZhciBidWlsdEluU3ltYm9scyA9IG5ldyBTZXQoT2JqZWN0LmdldE93blByb3BlcnR5TmFtZXMoU3ltYm9sKS5tYXAoKGtleSkgPT4gU3ltYm9sW2tleV0pLmZpbHRlcihpc1N5bWJvbCkpO1xudmFyIGdldDIgPSAvKiBAX19QVVJFX18gKi8gY3JlYXRlR2V0dGVyKCk7XG52YXIgc2hhbGxvd0dldCA9IC8qIEBfX1BVUkVfXyAqLyBjcmVhdGVHZXR0ZXIoZmFsc2UsIHRydWUpO1xudmFyIHJlYWRvbmx5R2V0ID0gLyogQF9fUFVSRV9fICovIGNyZWF0ZUdldHRlcih0cnVlKTtcbnZhciBzaGFsbG93UmVhZG9ubHlHZXQgPSAvKiBAX19QVVJFX18gKi8gY3JlYXRlR2V0dGVyKHRydWUsIHRydWUpO1xudmFyIGFycmF5SW5zdHJ1bWVudGF0aW9ucyA9IHt9O1xuW1wiaW5jbHVkZXNcIiwgXCJpbmRleE9mXCIsIFwibGFzdEluZGV4T2ZcIl0uZm9yRWFjaCgoa2V5KSA9PiB7XG4gIGNvbnN0IG1ldGhvZCA9IEFycmF5LnByb3RvdHlwZVtrZXldO1xuICBhcnJheUluc3RydW1lbnRhdGlvbnNba2V5XSA9IGZ1bmN0aW9uKC4uLmFyZ3MpIHtcbiAgICBjb25zdCBhcnIgPSB0b1Jhdyh0aGlzKTtcbiAgICBmb3IgKGxldCBpID0gMCwgbCA9IHRoaXMubGVuZ3RoOyBpIDwgbDsgaSsrKSB7XG4gICAgICB0cmFjayhhcnIsIFwiZ2V0XCIsIGkgKyBcIlwiKTtcbiAgICB9XG4gICAgY29uc3QgcmVzID0gbWV0aG9kLmFwcGx5KGFyciwgYXJncyk7XG4gICAgaWYgKHJlcyA9PT0gLTEgfHwgcmVzID09PSBmYWxzZSkge1xuICAgICAgcmV0dXJuIG1ldGhvZC5hcHBseShhcnIsIGFyZ3MubWFwKHRvUmF3KSk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHJldHVybiByZXM7XG4gICAgfVxuICB9O1xufSk7XG5bXCJwdXNoXCIsIFwicG9wXCIsIFwic2hpZnRcIiwgXCJ1bnNoaWZ0XCIsIFwic3BsaWNlXCJdLmZvckVhY2goKGtleSkgPT4ge1xuICBjb25zdCBtZXRob2QgPSBBcnJheS5wcm90b3R5cGVba2V5XTtcbiAgYXJyYXlJbnN0cnVtZW50YXRpb25zW2tleV0gPSBmdW5jdGlvbiguLi5hcmdzKSB7XG4gICAgcGF1c2VUcmFja2luZygpO1xuICAgIGNvbnN0IHJlcyA9IG1ldGhvZC5hcHBseSh0aGlzLCBhcmdzKTtcbiAgICByZXNldFRyYWNraW5nKCk7XG4gICAgcmV0dXJuIHJlcztcbiAgfTtcbn0pO1xuZnVuY3Rpb24gY3JlYXRlR2V0dGVyKGlzUmVhZG9ubHkgPSBmYWxzZSwgc2hhbGxvdyA9IGZhbHNlKSB7XG4gIHJldHVybiBmdW5jdGlvbiBnZXQzKHRhcmdldCwga2V5LCByZWNlaXZlcikge1xuICAgIGlmIChrZXkgPT09IFwiX192X2lzUmVhY3RpdmVcIikge1xuICAgICAgcmV0dXJuICFpc1JlYWRvbmx5O1xuICAgIH0gZWxzZSBpZiAoa2V5ID09PSBcIl9fdl9pc1JlYWRvbmx5XCIpIHtcbiAgICAgIHJldHVybiBpc1JlYWRvbmx5O1xuICAgIH0gZWxzZSBpZiAoa2V5ID09PSBcIl9fdl9yYXdcIiAmJiByZWNlaXZlciA9PT0gKGlzUmVhZG9ubHkgPyBzaGFsbG93ID8gc2hhbGxvd1JlYWRvbmx5TWFwIDogcmVhZG9ubHlNYXAgOiBzaGFsbG93ID8gc2hhbGxvd1JlYWN0aXZlTWFwIDogcmVhY3RpdmVNYXApLmdldCh0YXJnZXQpKSB7XG4gICAgICByZXR1cm4gdGFyZ2V0O1xuICAgIH1cbiAgICBjb25zdCB0YXJnZXRJc0FycmF5ID0gaXNBcnJheSh0YXJnZXQpO1xuICAgIGlmICghaXNSZWFkb25seSAmJiB0YXJnZXRJc0FycmF5ICYmIGhhc093bihhcnJheUluc3RydW1lbnRhdGlvbnMsIGtleSkpIHtcbiAgICAgIHJldHVybiBSZWZsZWN0LmdldChhcnJheUluc3RydW1lbnRhdGlvbnMsIGtleSwgcmVjZWl2ZXIpO1xuICAgIH1cbiAgICBjb25zdCByZXMgPSBSZWZsZWN0LmdldCh0YXJnZXQsIGtleSwgcmVjZWl2ZXIpO1xuICAgIGlmIChpc1N5bWJvbChrZXkpID8gYnVpbHRJblN5bWJvbHMuaGFzKGtleSkgOiBpc05vblRyYWNrYWJsZUtleXMoa2V5KSkge1xuICAgICAgcmV0dXJuIHJlcztcbiAgICB9XG4gICAgaWYgKCFpc1JlYWRvbmx5KSB7XG4gICAgICB0cmFjayh0YXJnZXQsIFwiZ2V0XCIsIGtleSk7XG4gICAgfVxuICAgIGlmIChzaGFsbG93KSB7XG4gICAgICByZXR1cm4gcmVzO1xuICAgIH1cbiAgICBpZiAoaXNSZWYocmVzKSkge1xuICAgICAgY29uc3Qgc2hvdWxkVW53cmFwID0gIXRhcmdldElzQXJyYXkgfHwgIWlzSW50ZWdlcktleShrZXkpO1xuICAgICAgcmV0dXJuIHNob3VsZFVud3JhcCA/IHJlcy52YWx1ZSA6IHJlcztcbiAgICB9XG4gICAgaWYgKGlzT2JqZWN0KHJlcykpIHtcbiAgICAgIHJldHVybiBpc1JlYWRvbmx5ID8gcmVhZG9ubHkocmVzKSA6IHJlYWN0aXZlMihyZXMpO1xuICAgIH1cbiAgICByZXR1cm4gcmVzO1xuICB9O1xufVxudmFyIHNldDIgPSAvKiBAX19QVVJFX18gKi8gY3JlYXRlU2V0dGVyKCk7XG52YXIgc2hhbGxvd1NldCA9IC8qIEBfX1BVUkVfXyAqLyBjcmVhdGVTZXR0ZXIodHJ1ZSk7XG5mdW5jdGlvbiBjcmVhdGVTZXR0ZXIoc2hhbGxvdyA9IGZhbHNlKSB7XG4gIHJldHVybiBmdW5jdGlvbiBzZXQzKHRhcmdldCwga2V5LCB2YWx1ZSwgcmVjZWl2ZXIpIHtcbiAgICBsZXQgb2xkVmFsdWUgPSB0YXJnZXRba2V5XTtcbiAgICBpZiAoIXNoYWxsb3cpIHtcbiAgICAgIHZhbHVlID0gdG9SYXcodmFsdWUpO1xuICAgICAgb2xkVmFsdWUgPSB0b1JhdyhvbGRWYWx1ZSk7XG4gICAgICBpZiAoIWlzQXJyYXkodGFyZ2V0KSAmJiBpc1JlZihvbGRWYWx1ZSkgJiYgIWlzUmVmKHZhbHVlKSkge1xuICAgICAgICBvbGRWYWx1ZS52YWx1ZSA9IHZhbHVlO1xuICAgICAgICByZXR1cm4gdHJ1ZTtcbiAgICAgIH1cbiAgICB9XG4gICAgY29uc3QgaGFkS2V5ID0gaXNBcnJheSh0YXJnZXQpICYmIGlzSW50ZWdlcktleShrZXkpID8gTnVtYmVyKGtleSkgPCB0YXJnZXQubGVuZ3RoIDogaGFzT3duKHRhcmdldCwga2V5KTtcbiAgICBjb25zdCByZXN1bHQgPSBSZWZsZWN0LnNldCh0YXJnZXQsIGtleSwgdmFsdWUsIHJlY2VpdmVyKTtcbiAgICBpZiAodGFyZ2V0ID09PSB0b1JhdyhyZWNlaXZlcikpIHtcbiAgICAgIGlmICghaGFkS2V5KSB7XG4gICAgICAgIHRyaWdnZXIodGFyZ2V0LCBcImFkZFwiLCBrZXksIHZhbHVlKTtcbiAgICAgIH0gZWxzZSBpZiAoaGFzQ2hhbmdlZCh2YWx1ZSwgb2xkVmFsdWUpKSB7XG4gICAgICAgIHRyaWdnZXIodGFyZ2V0LCBcInNldFwiLCBrZXksIHZhbHVlLCBvbGRWYWx1ZSk7XG4gICAgICB9XG4gICAgfVxuICAgIHJldHVybiByZXN1bHQ7XG4gIH07XG59XG5mdW5jdGlvbiBkZWxldGVQcm9wZXJ0eSh0YXJnZXQsIGtleSkge1xuICBjb25zdCBoYWRLZXkgPSBoYXNPd24odGFyZ2V0LCBrZXkpO1xuICBjb25zdCBvbGRWYWx1ZSA9IHRhcmdldFtrZXldO1xuICBjb25zdCByZXN1bHQgPSBSZWZsZWN0LmRlbGV0ZVByb3BlcnR5KHRhcmdldCwga2V5KTtcbiAgaWYgKHJlc3VsdCAmJiBoYWRLZXkpIHtcbiAgICB0cmlnZ2VyKHRhcmdldCwgXCJkZWxldGVcIiwga2V5LCB2b2lkIDAsIG9sZFZhbHVlKTtcbiAgfVxuICByZXR1cm4gcmVzdWx0O1xufVxuZnVuY3Rpb24gaGFzKHRhcmdldCwga2V5KSB7XG4gIGNvbnN0IHJlc3VsdCA9IFJlZmxlY3QuaGFzKHRhcmdldCwga2V5KTtcbiAgaWYgKCFpc1N5bWJvbChrZXkpIHx8ICFidWlsdEluU3ltYm9scy5oYXMoa2V5KSkge1xuICAgIHRyYWNrKHRhcmdldCwgXCJoYXNcIiwga2V5KTtcbiAgfVxuICByZXR1cm4gcmVzdWx0O1xufVxuZnVuY3Rpb24gb3duS2V5cyh0YXJnZXQpIHtcbiAgdHJhY2sodGFyZ2V0LCBcIml0ZXJhdGVcIiwgaXNBcnJheSh0YXJnZXQpID8gXCJsZW5ndGhcIiA6IElURVJBVEVfS0VZKTtcbiAgcmV0dXJuIFJlZmxlY3Qub3duS2V5cyh0YXJnZXQpO1xufVxudmFyIG11dGFibGVIYW5kbGVycyA9IHtcbiAgZ2V0OiBnZXQyLFxuICBzZXQ6IHNldDIsXG4gIGRlbGV0ZVByb3BlcnR5LFxuICBoYXMsXG4gIG93bktleXNcbn07XG52YXIgcmVhZG9ubHlIYW5kbGVycyA9IHtcbiAgZ2V0OiByZWFkb25seUdldCxcbiAgc2V0KHRhcmdldCwga2V5KSB7XG4gICAgaWYgKHRydWUpIHtcbiAgICAgIGNvbnNvbGUud2FybihgU2V0IG9wZXJhdGlvbiBvbiBrZXkgXCIke1N0cmluZyhrZXkpfVwiIGZhaWxlZDogdGFyZ2V0IGlzIHJlYWRvbmx5LmAsIHRhcmdldCk7XG4gICAgfVxuICAgIHJldHVybiB0cnVlO1xuICB9LFxuICBkZWxldGVQcm9wZXJ0eSh0YXJnZXQsIGtleSkge1xuICAgIGlmICh0cnVlKSB7XG4gICAgICBjb25zb2xlLndhcm4oYERlbGV0ZSBvcGVyYXRpb24gb24ga2V5IFwiJHtTdHJpbmcoa2V5KX1cIiBmYWlsZWQ6IHRhcmdldCBpcyByZWFkb25seS5gLCB0YXJnZXQpO1xuICAgIH1cbiAgICByZXR1cm4gdHJ1ZTtcbiAgfVxufTtcbnZhciBzaGFsbG93UmVhY3RpdmVIYW5kbGVycyA9IGV4dGVuZCh7fSwgbXV0YWJsZUhhbmRsZXJzLCB7XG4gIGdldDogc2hhbGxvd0dldCxcbiAgc2V0OiBzaGFsbG93U2V0XG59KTtcbnZhciBzaGFsbG93UmVhZG9ubHlIYW5kbGVycyA9IGV4dGVuZCh7fSwgcmVhZG9ubHlIYW5kbGVycywge1xuICBnZXQ6IHNoYWxsb3dSZWFkb25seUdldFxufSk7XG52YXIgdG9SZWFjdGl2ZSA9ICh2YWx1ZSkgPT4gaXNPYmplY3QodmFsdWUpID8gcmVhY3RpdmUyKHZhbHVlKSA6IHZhbHVlO1xudmFyIHRvUmVhZG9ubHkgPSAodmFsdWUpID0+IGlzT2JqZWN0KHZhbHVlKSA/IHJlYWRvbmx5KHZhbHVlKSA6IHZhbHVlO1xudmFyIHRvU2hhbGxvdyA9ICh2YWx1ZSkgPT4gdmFsdWU7XG52YXIgZ2V0UHJvdG8gPSAodikgPT4gUmVmbGVjdC5nZXRQcm90b3R5cGVPZih2KTtcbmZ1bmN0aW9uIGdldCQxKHRhcmdldCwga2V5LCBpc1JlYWRvbmx5ID0gZmFsc2UsIGlzU2hhbGxvdyA9IGZhbHNlKSB7XG4gIHRhcmdldCA9IHRhcmdldFtcIl9fdl9yYXdcIl07XG4gIGNvbnN0IHJhd1RhcmdldCA9IHRvUmF3KHRhcmdldCk7XG4gIGNvbnN0IHJhd0tleSA9IHRvUmF3KGtleSk7XG4gIGlmIChrZXkgIT09IHJhd0tleSkge1xuICAgICFpc1JlYWRvbmx5ICYmIHRyYWNrKHJhd1RhcmdldCwgXCJnZXRcIiwga2V5KTtcbiAgfVxuICAhaXNSZWFkb25seSAmJiB0cmFjayhyYXdUYXJnZXQsIFwiZ2V0XCIsIHJhd0tleSk7XG4gIGNvbnN0IHtoYXM6IGhhczJ9ID0gZ2V0UHJvdG8ocmF3VGFyZ2V0KTtcbiAgY29uc3Qgd3JhcCA9IGlzU2hhbGxvdyA/IHRvU2hhbGxvdyA6IGlzUmVhZG9ubHkgPyB0b1JlYWRvbmx5IDogdG9SZWFjdGl2ZTtcbiAgaWYgKGhhczIuY2FsbChyYXdUYXJnZXQsIGtleSkpIHtcbiAgICByZXR1cm4gd3JhcCh0YXJnZXQuZ2V0KGtleSkpO1xuICB9IGVsc2UgaWYgKGhhczIuY2FsbChyYXdUYXJnZXQsIHJhd0tleSkpIHtcbiAgICByZXR1cm4gd3JhcCh0YXJnZXQuZ2V0KHJhd0tleSkpO1xuICB9IGVsc2UgaWYgKHRhcmdldCAhPT0gcmF3VGFyZ2V0KSB7XG4gICAgdGFyZ2V0LmdldChrZXkpO1xuICB9XG59XG5mdW5jdGlvbiBoYXMkMShrZXksIGlzUmVhZG9ubHkgPSBmYWxzZSkge1xuICBjb25zdCB0YXJnZXQgPSB0aGlzW1wiX192X3Jhd1wiXTtcbiAgY29uc3QgcmF3VGFyZ2V0ID0gdG9SYXcodGFyZ2V0KTtcbiAgY29uc3QgcmF3S2V5ID0gdG9SYXcoa2V5KTtcbiAgaWYgKGtleSAhPT0gcmF3S2V5KSB7XG4gICAgIWlzUmVhZG9ubHkgJiYgdHJhY2socmF3VGFyZ2V0LCBcImhhc1wiLCBrZXkpO1xuICB9XG4gICFpc1JlYWRvbmx5ICYmIHRyYWNrKHJhd1RhcmdldCwgXCJoYXNcIiwgcmF3S2V5KTtcbiAgcmV0dXJuIGtleSA9PT0gcmF3S2V5ID8gdGFyZ2V0LmhhcyhrZXkpIDogdGFyZ2V0LmhhcyhrZXkpIHx8IHRhcmdldC5oYXMocmF3S2V5KTtcbn1cbmZ1bmN0aW9uIHNpemUodGFyZ2V0LCBpc1JlYWRvbmx5ID0gZmFsc2UpIHtcbiAgdGFyZ2V0ID0gdGFyZ2V0W1wiX192X3Jhd1wiXTtcbiAgIWlzUmVhZG9ubHkgJiYgdHJhY2sodG9SYXcodGFyZ2V0KSwgXCJpdGVyYXRlXCIsIElURVJBVEVfS0VZKTtcbiAgcmV0dXJuIFJlZmxlY3QuZ2V0KHRhcmdldCwgXCJzaXplXCIsIHRhcmdldCk7XG59XG5mdW5jdGlvbiBhZGQodmFsdWUpIHtcbiAgdmFsdWUgPSB0b1Jhdyh2YWx1ZSk7XG4gIGNvbnN0IHRhcmdldCA9IHRvUmF3KHRoaXMpO1xuICBjb25zdCBwcm90byA9IGdldFByb3RvKHRhcmdldCk7XG4gIGNvbnN0IGhhZEtleSA9IHByb3RvLmhhcy5jYWxsKHRhcmdldCwgdmFsdWUpO1xuICBpZiAoIWhhZEtleSkge1xuICAgIHRhcmdldC5hZGQodmFsdWUpO1xuICAgIHRyaWdnZXIodGFyZ2V0LCBcImFkZFwiLCB2YWx1ZSwgdmFsdWUpO1xuICB9XG4gIHJldHVybiB0aGlzO1xufVxuZnVuY3Rpb24gc2V0JDEoa2V5LCB2YWx1ZSkge1xuICB2YWx1ZSA9IHRvUmF3KHZhbHVlKTtcbiAgY29uc3QgdGFyZ2V0ID0gdG9SYXcodGhpcyk7XG4gIGNvbnN0IHtoYXM6IGhhczIsIGdldDogZ2V0M30gPSBnZXRQcm90byh0YXJnZXQpO1xuICBsZXQgaGFkS2V5ID0gaGFzMi5jYWxsKHRhcmdldCwga2V5KTtcbiAgaWYgKCFoYWRLZXkpIHtcbiAgICBrZXkgPSB0b1JhdyhrZXkpO1xuICAgIGhhZEtleSA9IGhhczIuY2FsbCh0YXJnZXQsIGtleSk7XG4gIH0gZWxzZSBpZiAodHJ1ZSkge1xuICAgIGNoZWNrSWRlbnRpdHlLZXlzKHRhcmdldCwgaGFzMiwga2V5KTtcbiAgfVxuICBjb25zdCBvbGRWYWx1ZSA9IGdldDMuY2FsbCh0YXJnZXQsIGtleSk7XG4gIHRhcmdldC5zZXQoa2V5LCB2YWx1ZSk7XG4gIGlmICghaGFkS2V5KSB7XG4gICAgdHJpZ2dlcih0YXJnZXQsIFwiYWRkXCIsIGtleSwgdmFsdWUpO1xuICB9IGVsc2UgaWYgKGhhc0NoYW5nZWQodmFsdWUsIG9sZFZhbHVlKSkge1xuICAgIHRyaWdnZXIodGFyZ2V0LCBcInNldFwiLCBrZXksIHZhbHVlLCBvbGRWYWx1ZSk7XG4gIH1cbiAgcmV0dXJuIHRoaXM7XG59XG5mdW5jdGlvbiBkZWxldGVFbnRyeShrZXkpIHtcbiAgY29uc3QgdGFyZ2V0ID0gdG9SYXcodGhpcyk7XG4gIGNvbnN0IHtoYXM6IGhhczIsIGdldDogZ2V0M30gPSBnZXRQcm90byh0YXJnZXQpO1xuICBsZXQgaGFkS2V5ID0gaGFzMi5jYWxsKHRhcmdldCwga2V5KTtcbiAgaWYgKCFoYWRLZXkpIHtcbiAgICBrZXkgPSB0b1JhdyhrZXkpO1xuICAgIGhhZEtleSA9IGhhczIuY2FsbCh0YXJnZXQsIGtleSk7XG4gIH0gZWxzZSBpZiAodHJ1ZSkge1xuICAgIGNoZWNrSWRlbnRpdHlLZXlzKHRhcmdldCwgaGFzMiwga2V5KTtcbiAgfVxuICBjb25zdCBvbGRWYWx1ZSA9IGdldDMgPyBnZXQzLmNhbGwodGFyZ2V0LCBrZXkpIDogdm9pZCAwO1xuICBjb25zdCByZXN1bHQgPSB0YXJnZXQuZGVsZXRlKGtleSk7XG4gIGlmIChoYWRLZXkpIHtcbiAgICB0cmlnZ2VyKHRhcmdldCwgXCJkZWxldGVcIiwga2V5LCB2b2lkIDAsIG9sZFZhbHVlKTtcbiAgfVxuICByZXR1cm4gcmVzdWx0O1xufVxuZnVuY3Rpb24gY2xlYXIoKSB7XG4gIGNvbnN0IHRhcmdldCA9IHRvUmF3KHRoaXMpO1xuICBjb25zdCBoYWRJdGVtcyA9IHRhcmdldC5zaXplICE9PSAwO1xuICBjb25zdCBvbGRUYXJnZXQgPSB0cnVlID8gaXNNYXAodGFyZ2V0KSA/IG5ldyBNYXAodGFyZ2V0KSA6IG5ldyBTZXQodGFyZ2V0KSA6IHZvaWQgMDtcbiAgY29uc3QgcmVzdWx0ID0gdGFyZ2V0LmNsZWFyKCk7XG4gIGlmIChoYWRJdGVtcykge1xuICAgIHRyaWdnZXIodGFyZ2V0LCBcImNsZWFyXCIsIHZvaWQgMCwgdm9pZCAwLCBvbGRUYXJnZXQpO1xuICB9XG4gIHJldHVybiByZXN1bHQ7XG59XG5mdW5jdGlvbiBjcmVhdGVGb3JFYWNoKGlzUmVhZG9ubHksIGlzU2hhbGxvdykge1xuICByZXR1cm4gZnVuY3Rpb24gZm9yRWFjaChjYWxsYmFjaywgdGhpc0FyZykge1xuICAgIGNvbnN0IG9ic2VydmVkID0gdGhpcztcbiAgICBjb25zdCB0YXJnZXQgPSBvYnNlcnZlZFtcIl9fdl9yYXdcIl07XG4gICAgY29uc3QgcmF3VGFyZ2V0ID0gdG9SYXcodGFyZ2V0KTtcbiAgICBjb25zdCB3cmFwID0gaXNTaGFsbG93ID8gdG9TaGFsbG93IDogaXNSZWFkb25seSA/IHRvUmVhZG9ubHkgOiB0b1JlYWN0aXZlO1xuICAgICFpc1JlYWRvbmx5ICYmIHRyYWNrKHJhd1RhcmdldCwgXCJpdGVyYXRlXCIsIElURVJBVEVfS0VZKTtcbiAgICByZXR1cm4gdGFyZ2V0LmZvckVhY2goKHZhbHVlLCBrZXkpID0+IHtcbiAgICAgIHJldHVybiBjYWxsYmFjay5jYWxsKHRoaXNBcmcsIHdyYXAodmFsdWUpLCB3cmFwKGtleSksIG9ic2VydmVkKTtcbiAgICB9KTtcbiAgfTtcbn1cbmZ1bmN0aW9uIGNyZWF0ZUl0ZXJhYmxlTWV0aG9kKG1ldGhvZCwgaXNSZWFkb25seSwgaXNTaGFsbG93KSB7XG4gIHJldHVybiBmdW5jdGlvbiguLi5hcmdzKSB7XG4gICAgY29uc3QgdGFyZ2V0ID0gdGhpc1tcIl9fdl9yYXdcIl07XG4gICAgY29uc3QgcmF3VGFyZ2V0ID0gdG9SYXcodGFyZ2V0KTtcbiAgICBjb25zdCB0YXJnZXRJc01hcCA9IGlzTWFwKHJhd1RhcmdldCk7XG4gICAgY29uc3QgaXNQYWlyID0gbWV0aG9kID09PSBcImVudHJpZXNcIiB8fCBtZXRob2QgPT09IFN5bWJvbC5pdGVyYXRvciAmJiB0YXJnZXRJc01hcDtcbiAgICBjb25zdCBpc0tleU9ubHkgPSBtZXRob2QgPT09IFwia2V5c1wiICYmIHRhcmdldElzTWFwO1xuICAgIGNvbnN0IGlubmVySXRlcmF0b3IgPSB0YXJnZXRbbWV0aG9kXSguLi5hcmdzKTtcbiAgICBjb25zdCB3cmFwID0gaXNTaGFsbG93ID8gdG9TaGFsbG93IDogaXNSZWFkb25seSA/IHRvUmVhZG9ubHkgOiB0b1JlYWN0aXZlO1xuICAgICFpc1JlYWRvbmx5ICYmIHRyYWNrKHJhd1RhcmdldCwgXCJpdGVyYXRlXCIsIGlzS2V5T25seSA/IE1BUF9LRVlfSVRFUkFURV9LRVkgOiBJVEVSQVRFX0tFWSk7XG4gICAgcmV0dXJuIHtcbiAgICAgIG5leHQoKSB7XG4gICAgICAgIGNvbnN0IHt2YWx1ZSwgZG9uZX0gPSBpbm5lckl0ZXJhdG9yLm5leHQoKTtcbiAgICAgICAgcmV0dXJuIGRvbmUgPyB7dmFsdWUsIGRvbmV9IDoge1xuICAgICAgICAgIHZhbHVlOiBpc1BhaXIgPyBbd3JhcCh2YWx1ZVswXSksIHdyYXAodmFsdWVbMV0pXSA6IHdyYXAodmFsdWUpLFxuICAgICAgICAgIGRvbmVcbiAgICAgICAgfTtcbiAgICAgIH0sXG4gICAgICBbU3ltYm9sLml0ZXJhdG9yXSgpIHtcbiAgICAgICAgcmV0dXJuIHRoaXM7XG4gICAgICB9XG4gICAgfTtcbiAgfTtcbn1cbmZ1bmN0aW9uIGNyZWF0ZVJlYWRvbmx5TWV0aG9kKHR5cGUpIHtcbiAgcmV0dXJuIGZ1bmN0aW9uKC4uLmFyZ3MpIHtcbiAgICBpZiAodHJ1ZSkge1xuICAgICAgY29uc3Qga2V5ID0gYXJnc1swXSA/IGBvbiBrZXkgXCIke2FyZ3NbMF19XCIgYCA6IGBgO1xuICAgICAgY29uc29sZS53YXJuKGAke2NhcGl0YWxpemUodHlwZSl9IG9wZXJhdGlvbiAke2tleX1mYWlsZWQ6IHRhcmdldCBpcyByZWFkb25seS5gLCB0b1Jhdyh0aGlzKSk7XG4gICAgfVxuICAgIHJldHVybiB0eXBlID09PSBcImRlbGV0ZVwiID8gZmFsc2UgOiB0aGlzO1xuICB9O1xufVxudmFyIG11dGFibGVJbnN0cnVtZW50YXRpb25zID0ge1xuICBnZXQoa2V5KSB7XG4gICAgcmV0dXJuIGdldCQxKHRoaXMsIGtleSk7XG4gIH0sXG4gIGdldCBzaXplKCkge1xuICAgIHJldHVybiBzaXplKHRoaXMpO1xuICB9LFxuICBoYXM6IGhhcyQxLFxuICBhZGQsXG4gIHNldDogc2V0JDEsXG4gIGRlbGV0ZTogZGVsZXRlRW50cnksXG4gIGNsZWFyLFxuICBmb3JFYWNoOiBjcmVhdGVGb3JFYWNoKGZhbHNlLCBmYWxzZSlcbn07XG52YXIgc2hhbGxvd0luc3RydW1lbnRhdGlvbnMgPSB7XG4gIGdldChrZXkpIHtcbiAgICByZXR1cm4gZ2V0JDEodGhpcywga2V5LCBmYWxzZSwgdHJ1ZSk7XG4gIH0sXG4gIGdldCBzaXplKCkge1xuICAgIHJldHVybiBzaXplKHRoaXMpO1xuICB9LFxuICBoYXM6IGhhcyQxLFxuICBhZGQsXG4gIHNldDogc2V0JDEsXG4gIGRlbGV0ZTogZGVsZXRlRW50cnksXG4gIGNsZWFyLFxuICBmb3JFYWNoOiBjcmVhdGVGb3JFYWNoKGZhbHNlLCB0cnVlKVxufTtcbnZhciByZWFkb25seUluc3RydW1lbnRhdGlvbnMgPSB7XG4gIGdldChrZXkpIHtcbiAgICByZXR1cm4gZ2V0JDEodGhpcywga2V5LCB0cnVlKTtcbiAgfSxcbiAgZ2V0IHNpemUoKSB7XG4gICAgcmV0dXJuIHNpemUodGhpcywgdHJ1ZSk7XG4gIH0sXG4gIGhhcyhrZXkpIHtcbiAgICByZXR1cm4gaGFzJDEuY2FsbCh0aGlzLCBrZXksIHRydWUpO1xuICB9LFxuICBhZGQ6IGNyZWF0ZVJlYWRvbmx5TWV0aG9kKFwiYWRkXCIpLFxuICBzZXQ6IGNyZWF0ZVJlYWRvbmx5TWV0aG9kKFwic2V0XCIpLFxuICBkZWxldGU6IGNyZWF0ZVJlYWRvbmx5TWV0aG9kKFwiZGVsZXRlXCIpLFxuICBjbGVhcjogY3JlYXRlUmVhZG9ubHlNZXRob2QoXCJjbGVhclwiKSxcbiAgZm9yRWFjaDogY3JlYXRlRm9yRWFjaCh0cnVlLCBmYWxzZSlcbn07XG52YXIgc2hhbGxvd1JlYWRvbmx5SW5zdHJ1bWVudGF0aW9ucyA9IHtcbiAgZ2V0KGtleSkge1xuICAgIHJldHVybiBnZXQkMSh0aGlzLCBrZXksIHRydWUsIHRydWUpO1xuICB9LFxuICBnZXQgc2l6ZSgpIHtcbiAgICByZXR1cm4gc2l6ZSh0aGlzLCB0cnVlKTtcbiAgfSxcbiAgaGFzKGtleSkge1xuICAgIHJldHVybiBoYXMkMS5jYWxsKHRoaXMsIGtleSwgdHJ1ZSk7XG4gIH0sXG4gIGFkZDogY3JlYXRlUmVhZG9ubHlNZXRob2QoXCJhZGRcIiksXG4gIHNldDogY3JlYXRlUmVhZG9ubHlNZXRob2QoXCJzZXRcIiksXG4gIGRlbGV0ZTogY3JlYXRlUmVhZG9ubHlNZXRob2QoXCJkZWxldGVcIiksXG4gIGNsZWFyOiBjcmVhdGVSZWFkb25seU1ldGhvZChcImNsZWFyXCIpLFxuICBmb3JFYWNoOiBjcmVhdGVGb3JFYWNoKHRydWUsIHRydWUpXG59O1xudmFyIGl0ZXJhdG9yTWV0aG9kcyA9IFtcImtleXNcIiwgXCJ2YWx1ZXNcIiwgXCJlbnRyaWVzXCIsIFN5bWJvbC5pdGVyYXRvcl07XG5pdGVyYXRvck1ldGhvZHMuZm9yRWFjaCgobWV0aG9kKSA9PiB7XG4gIG11dGFibGVJbnN0cnVtZW50YXRpb25zW21ldGhvZF0gPSBjcmVhdGVJdGVyYWJsZU1ldGhvZChtZXRob2QsIGZhbHNlLCBmYWxzZSk7XG4gIHJlYWRvbmx5SW5zdHJ1bWVudGF0aW9uc1ttZXRob2RdID0gY3JlYXRlSXRlcmFibGVNZXRob2QobWV0aG9kLCB0cnVlLCBmYWxzZSk7XG4gIHNoYWxsb3dJbnN0cnVtZW50YXRpb25zW21ldGhvZF0gPSBjcmVhdGVJdGVyYWJsZU1ldGhvZChtZXRob2QsIGZhbHNlLCB0cnVlKTtcbiAgc2hhbGxvd1JlYWRvbmx5SW5zdHJ1bWVudGF0aW9uc1ttZXRob2RdID0gY3JlYXRlSXRlcmFibGVNZXRob2QobWV0aG9kLCB0cnVlLCB0cnVlKTtcbn0pO1xuZnVuY3Rpb24gY3JlYXRlSW5zdHJ1bWVudGF0aW9uR2V0dGVyKGlzUmVhZG9ubHksIHNoYWxsb3cpIHtcbiAgY29uc3QgaW5zdHJ1bWVudGF0aW9ucyA9IHNoYWxsb3cgPyBpc1JlYWRvbmx5ID8gc2hhbGxvd1JlYWRvbmx5SW5zdHJ1bWVudGF0aW9ucyA6IHNoYWxsb3dJbnN0cnVtZW50YXRpb25zIDogaXNSZWFkb25seSA/IHJlYWRvbmx5SW5zdHJ1bWVudGF0aW9ucyA6IG11dGFibGVJbnN0cnVtZW50YXRpb25zO1xuICByZXR1cm4gKHRhcmdldCwga2V5LCByZWNlaXZlcikgPT4ge1xuICAgIGlmIChrZXkgPT09IFwiX192X2lzUmVhY3RpdmVcIikge1xuICAgICAgcmV0dXJuICFpc1JlYWRvbmx5O1xuICAgIH0gZWxzZSBpZiAoa2V5ID09PSBcIl9fdl9pc1JlYWRvbmx5XCIpIHtcbiAgICAgIHJldHVybiBpc1JlYWRvbmx5O1xuICAgIH0gZWxzZSBpZiAoa2V5ID09PSBcIl9fdl9yYXdcIikge1xuICAgICAgcmV0dXJuIHRhcmdldDtcbiAgICB9XG4gICAgcmV0dXJuIFJlZmxlY3QuZ2V0KGhhc093bihpbnN0cnVtZW50YXRpb25zLCBrZXkpICYmIGtleSBpbiB0YXJnZXQgPyBpbnN0cnVtZW50YXRpb25zIDogdGFyZ2V0LCBrZXksIHJlY2VpdmVyKTtcbiAgfTtcbn1cbnZhciBtdXRhYmxlQ29sbGVjdGlvbkhhbmRsZXJzID0ge1xuICBnZXQ6IGNyZWF0ZUluc3RydW1lbnRhdGlvbkdldHRlcihmYWxzZSwgZmFsc2UpXG59O1xudmFyIHNoYWxsb3dDb2xsZWN0aW9uSGFuZGxlcnMgPSB7XG4gIGdldDogY3JlYXRlSW5zdHJ1bWVudGF0aW9uR2V0dGVyKGZhbHNlLCB0cnVlKVxufTtcbnZhciByZWFkb25seUNvbGxlY3Rpb25IYW5kbGVycyA9IHtcbiAgZ2V0OiBjcmVhdGVJbnN0cnVtZW50YXRpb25HZXR0ZXIodHJ1ZSwgZmFsc2UpXG59O1xudmFyIHNoYWxsb3dSZWFkb25seUNvbGxlY3Rpb25IYW5kbGVycyA9IHtcbiAgZ2V0OiBjcmVhdGVJbnN0cnVtZW50YXRpb25HZXR0ZXIodHJ1ZSwgdHJ1ZSlcbn07XG5mdW5jdGlvbiBjaGVja0lkZW50aXR5S2V5cyh0YXJnZXQsIGhhczIsIGtleSkge1xuICBjb25zdCByYXdLZXkgPSB0b1JhdyhrZXkpO1xuICBpZiAocmF3S2V5ICE9PSBrZXkgJiYgaGFzMi5jYWxsKHRhcmdldCwgcmF3S2V5KSkge1xuICAgIGNvbnN0IHR5cGUgPSB0b1Jhd1R5cGUodGFyZ2V0KTtcbiAgICBjb25zb2xlLndhcm4oYFJlYWN0aXZlICR7dHlwZX0gY29udGFpbnMgYm90aCB0aGUgcmF3IGFuZCByZWFjdGl2ZSB2ZXJzaW9ucyBvZiB0aGUgc2FtZSBvYmplY3Qke3R5cGUgPT09IGBNYXBgID8gYCBhcyBrZXlzYCA6IGBgfSwgd2hpY2ggY2FuIGxlYWQgdG8gaW5jb25zaXN0ZW5jaWVzLiBBdm9pZCBkaWZmZXJlbnRpYXRpbmcgYmV0d2VlbiB0aGUgcmF3IGFuZCByZWFjdGl2ZSB2ZXJzaW9ucyBvZiBhbiBvYmplY3QgYW5kIG9ubHkgdXNlIHRoZSByZWFjdGl2ZSB2ZXJzaW9uIGlmIHBvc3NpYmxlLmApO1xuICB9XG59XG52YXIgcmVhY3RpdmVNYXAgPSBuZXcgV2Vha01hcCgpO1xudmFyIHNoYWxsb3dSZWFjdGl2ZU1hcCA9IG5ldyBXZWFrTWFwKCk7XG52YXIgcmVhZG9ubHlNYXAgPSBuZXcgV2Vha01hcCgpO1xudmFyIHNoYWxsb3dSZWFkb25seU1hcCA9IG5ldyBXZWFrTWFwKCk7XG5mdW5jdGlvbiB0YXJnZXRUeXBlTWFwKHJhd1R5cGUpIHtcbiAgc3dpdGNoIChyYXdUeXBlKSB7XG4gICAgY2FzZSBcIk9iamVjdFwiOlxuICAgIGNhc2UgXCJBcnJheVwiOlxuICAgICAgcmV0dXJuIDE7XG4gICAgY2FzZSBcIk1hcFwiOlxuICAgIGNhc2UgXCJTZXRcIjpcbiAgICBjYXNlIFwiV2Vha01hcFwiOlxuICAgIGNhc2UgXCJXZWFrU2V0XCI6XG4gICAgICByZXR1cm4gMjtcbiAgICBkZWZhdWx0OlxuICAgICAgcmV0dXJuIDA7XG4gIH1cbn1cbmZ1bmN0aW9uIGdldFRhcmdldFR5cGUodmFsdWUpIHtcbiAgcmV0dXJuIHZhbHVlW1wiX192X3NraXBcIl0gfHwgIU9iamVjdC5pc0V4dGVuc2libGUodmFsdWUpID8gMCA6IHRhcmdldFR5cGVNYXAodG9SYXdUeXBlKHZhbHVlKSk7XG59XG5mdW5jdGlvbiByZWFjdGl2ZTIodGFyZ2V0KSB7XG4gIGlmICh0YXJnZXQgJiYgdGFyZ2V0W1wiX192X2lzUmVhZG9ubHlcIl0pIHtcbiAgICByZXR1cm4gdGFyZ2V0O1xuICB9XG4gIHJldHVybiBjcmVhdGVSZWFjdGl2ZU9iamVjdCh0YXJnZXQsIGZhbHNlLCBtdXRhYmxlSGFuZGxlcnMsIG11dGFibGVDb2xsZWN0aW9uSGFuZGxlcnMsIHJlYWN0aXZlTWFwKTtcbn1cbmZ1bmN0aW9uIHJlYWRvbmx5KHRhcmdldCkge1xuICByZXR1cm4gY3JlYXRlUmVhY3RpdmVPYmplY3QodGFyZ2V0LCB0cnVlLCByZWFkb25seUhhbmRsZXJzLCByZWFkb25seUNvbGxlY3Rpb25IYW5kbGVycywgcmVhZG9ubHlNYXApO1xufVxuZnVuY3Rpb24gY3JlYXRlUmVhY3RpdmVPYmplY3QodGFyZ2V0LCBpc1JlYWRvbmx5LCBiYXNlSGFuZGxlcnMsIGNvbGxlY3Rpb25IYW5kbGVycywgcHJveHlNYXApIHtcbiAgaWYgKCFpc09iamVjdCh0YXJnZXQpKSB7XG4gICAgaWYgKHRydWUpIHtcbiAgICAgIGNvbnNvbGUud2FybihgdmFsdWUgY2Fubm90IGJlIG1hZGUgcmVhY3RpdmU6ICR7U3RyaW5nKHRhcmdldCl9YCk7XG4gICAgfVxuICAgIHJldHVybiB0YXJnZXQ7XG4gIH1cbiAgaWYgKHRhcmdldFtcIl9fdl9yYXdcIl0gJiYgIShpc1JlYWRvbmx5ICYmIHRhcmdldFtcIl9fdl9pc1JlYWN0aXZlXCJdKSkge1xuICAgIHJldHVybiB0YXJnZXQ7XG4gIH1cbiAgY29uc3QgZXhpc3RpbmdQcm94eSA9IHByb3h5TWFwLmdldCh0YXJnZXQpO1xuICBpZiAoZXhpc3RpbmdQcm94eSkge1xuICAgIHJldHVybiBleGlzdGluZ1Byb3h5O1xuICB9XG4gIGNvbnN0IHRhcmdldFR5cGUgPSBnZXRUYXJnZXRUeXBlKHRhcmdldCk7XG4gIGlmICh0YXJnZXRUeXBlID09PSAwKSB7XG4gICAgcmV0dXJuIHRhcmdldDtcbiAgfVxuICBjb25zdCBwcm94eSA9IG5ldyBQcm94eSh0YXJnZXQsIHRhcmdldFR5cGUgPT09IDIgPyBjb2xsZWN0aW9uSGFuZGxlcnMgOiBiYXNlSGFuZGxlcnMpO1xuICBwcm94eU1hcC5zZXQodGFyZ2V0LCBwcm94eSk7XG4gIHJldHVybiBwcm94eTtcbn1cbmZ1bmN0aW9uIHRvUmF3KG9ic2VydmVkKSB7XG4gIHJldHVybiBvYnNlcnZlZCAmJiB0b1JhdyhvYnNlcnZlZFtcIl9fdl9yYXdcIl0pIHx8IG9ic2VydmVkO1xufVxuZnVuY3Rpb24gaXNSZWYocikge1xuICByZXR1cm4gQm9vbGVhbihyICYmIHIuX192X2lzUmVmID09PSB0cnVlKTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL21hZ2ljcy8kbmV4dFRpY2suanNcbm1hZ2ljKFwibmV4dFRpY2tcIiwgKCkgPT4gbmV4dFRpY2spO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvbWFnaWNzLyRkaXNwYXRjaC5qc1xubWFnaWMoXCJkaXNwYXRjaFwiLCAoZWwpID0+IGRpc3BhdGNoLmJpbmQoZGlzcGF0Y2gsIGVsKSk7XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9tYWdpY3MvJHdhdGNoLmpzXG5tYWdpYyhcIndhdGNoXCIsIChlbCwge2V2YWx1YXRlTGF0ZXI6IGV2YWx1YXRlTGF0ZXIyLCBlZmZlY3Q6IGVmZmVjdDN9KSA9PiAoa2V5LCBjYWxsYmFjaykgPT4ge1xuICBsZXQgZXZhbHVhdGUyID0gZXZhbHVhdGVMYXRlcjIoa2V5KTtcbiAgbGV0IGZpcnN0VGltZSA9IHRydWU7XG4gIGxldCBvbGRWYWx1ZTtcbiAgbGV0IGVmZmVjdFJlZmVyZW5jZSA9IGVmZmVjdDMoKCkgPT4gZXZhbHVhdGUyKCh2YWx1ZSkgPT4ge1xuICAgIEpTT04uc3RyaW5naWZ5KHZhbHVlKTtcbiAgICBpZiAoIWZpcnN0VGltZSkge1xuICAgICAgcXVldWVNaWNyb3Rhc2soKCkgPT4ge1xuICAgICAgICBjYWxsYmFjayh2YWx1ZSwgb2xkVmFsdWUpO1xuICAgICAgICBvbGRWYWx1ZSA9IHZhbHVlO1xuICAgICAgfSk7XG4gICAgfSBlbHNlIHtcbiAgICAgIG9sZFZhbHVlID0gdmFsdWU7XG4gICAgfVxuICAgIGZpcnN0VGltZSA9IGZhbHNlO1xuICB9KSk7XG4gIGVsLl94X2VmZmVjdHMuZGVsZXRlKGVmZmVjdFJlZmVyZW5jZSk7XG59KTtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL21hZ2ljcy8kc3RvcmUuanNcbm1hZ2ljKFwic3RvcmVcIiwgZ2V0U3RvcmVzKTtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL21hZ2ljcy8kZGF0YS5qc1xubWFnaWMoXCJkYXRhXCIsIChlbCkgPT4gc2NvcGUoZWwpKTtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL21hZ2ljcy8kcm9vdC5qc1xubWFnaWMoXCJyb290XCIsIChlbCkgPT4gY2xvc2VzdFJvb3QoZWwpKTtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL21hZ2ljcy8kcmVmcy5qc1xubWFnaWMoXCJyZWZzXCIsIChlbCkgPT4ge1xuICBpZiAoZWwuX3hfcmVmc19wcm94eSlcbiAgICByZXR1cm4gZWwuX3hfcmVmc19wcm94eTtcbiAgZWwuX3hfcmVmc19wcm94eSA9IG1lcmdlUHJveGllcyhnZXRBcnJheU9mUmVmT2JqZWN0KGVsKSk7XG4gIHJldHVybiBlbC5feF9yZWZzX3Byb3h5O1xufSk7XG5mdW5jdGlvbiBnZXRBcnJheU9mUmVmT2JqZWN0KGVsKSB7XG4gIGxldCByZWZPYmplY3RzID0gW107XG4gIGxldCBjdXJyZW50RWwgPSBlbDtcbiAgd2hpbGUgKGN1cnJlbnRFbCkge1xuICAgIGlmIChjdXJyZW50RWwuX3hfcmVmcylcbiAgICAgIHJlZk9iamVjdHMucHVzaChjdXJyZW50RWwuX3hfcmVmcyk7XG4gICAgY3VycmVudEVsID0gY3VycmVudEVsLnBhcmVudE5vZGU7XG4gIH1cbiAgcmV0dXJuIHJlZk9iamVjdHM7XG59XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9pZHMuanNcbnZhciBnbG9iYWxJZE1lbW8gPSB7fTtcbmZ1bmN0aW9uIGZpbmRBbmRJbmNyZW1lbnRJZChuYW1lKSB7XG4gIGlmICghZ2xvYmFsSWRNZW1vW25hbWVdKVxuICAgIGdsb2JhbElkTWVtb1tuYW1lXSA9IDA7XG4gIHJldHVybiArK2dsb2JhbElkTWVtb1tuYW1lXTtcbn1cbmZ1bmN0aW9uIGNsb3Nlc3RJZFJvb3QoZWwsIG5hbWUpIHtcbiAgcmV0dXJuIGZpbmRDbG9zZXN0KGVsLCAoZWxlbWVudCkgPT4ge1xuICAgIGlmIChlbGVtZW50Ll94X2lkcyAmJiBlbGVtZW50Ll94X2lkc1tuYW1lXSlcbiAgICAgIHJldHVybiB0cnVlO1xuICB9KTtcbn1cbmZ1bmN0aW9uIHNldElkUm9vdChlbCwgbmFtZSkge1xuICBpZiAoIWVsLl94X2lkcylcbiAgICBlbC5feF9pZHMgPSB7fTtcbiAgaWYgKCFlbC5feF9pZHNbbmFtZV0pXG4gICAgZWwuX3hfaWRzW25hbWVdID0gZmluZEFuZEluY3JlbWVudElkKG5hbWUpO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvbWFnaWNzLyRpZC5qc1xubWFnaWMoXCJpZFwiLCAoZWwpID0+IChuYW1lLCBrZXkgPSBudWxsKSA9PiB7XG4gIGxldCByb290ID0gY2xvc2VzdElkUm9vdChlbCwgbmFtZSk7XG4gIGxldCBpZCA9IHJvb3QgPyByb290Ll94X2lkc1tuYW1lXSA6IGZpbmRBbmRJbmNyZW1lbnRJZChuYW1lKTtcbiAgcmV0dXJuIGtleSA/IGAke25hbWV9LSR7aWR9LSR7a2V5fWAgOiBgJHtuYW1lfS0ke2lkfWA7XG59KTtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL21hZ2ljcy8kZWwuanNcbm1hZ2ljKFwiZWxcIiwgKGVsKSA9PiBlbCk7XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9tYWdpY3MvaW5kZXguanNcbndhcm5NaXNzaW5nUGx1Z2luTWFnaWMoXCJGb2N1c1wiLCBcImZvY3VzXCIsIFwiZm9jdXNcIik7XG53YXJuTWlzc2luZ1BsdWdpbk1hZ2ljKFwiUGVyc2lzdFwiLCBcInBlcnNpc3RcIiwgXCJwZXJzaXN0XCIpO1xuZnVuY3Rpb24gd2Fybk1pc3NpbmdQbHVnaW5NYWdpYyhuYW1lLCBtYWdpY05hbWUsIHNsdWcpIHtcbiAgbWFnaWMobWFnaWNOYW1lLCAoZWwpID0+IHdhcm4oYFlvdSBjYW4ndCB1c2UgWyQke2RpcmVjdGl2ZU5hbWV9XSB3aXRob3V0IGZpcnN0IGluc3RhbGxpbmcgdGhlIFwiJHtuYW1lfVwiIHBsdWdpbiBoZXJlOiBodHRwczovL2FscGluZWpzLmRldi9wbHVnaW5zLyR7c2x1Z31gLCBlbCkpO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy94LW1vZGVsYWJsZS5qc1xuZGlyZWN0aXZlKFwibW9kZWxhYmxlXCIsIChlbCwge2V4cHJlc3Npb259LCB7ZWZmZWN0OiBlZmZlY3QzLCBldmFsdWF0ZUxhdGVyOiBldmFsdWF0ZUxhdGVyMn0pID0+IHtcbiAgbGV0IGZ1bmMgPSBldmFsdWF0ZUxhdGVyMihleHByZXNzaW9uKTtcbiAgbGV0IGlubmVyR2V0ID0gKCkgPT4ge1xuICAgIGxldCByZXN1bHQ7XG4gICAgZnVuYygoaSkgPT4gcmVzdWx0ID0gaSk7XG4gICAgcmV0dXJuIHJlc3VsdDtcbiAgfTtcbiAgbGV0IGV2YWx1YXRlSW5uZXJTZXQgPSBldmFsdWF0ZUxhdGVyMihgJHtleHByZXNzaW9ufSA9IF9fcGxhY2Vob2xkZXJgKTtcbiAgbGV0IGlubmVyU2V0ID0gKHZhbCkgPT4gZXZhbHVhdGVJbm5lclNldCgoKSA9PiB7XG4gIH0sIHtzY29wZToge19fcGxhY2Vob2xkZXI6IHZhbH19KTtcbiAgbGV0IGluaXRpYWxWYWx1ZSA9IGlubmVyR2V0KCk7XG4gIGlubmVyU2V0KGluaXRpYWxWYWx1ZSk7XG4gIHF1ZXVlTWljcm90YXNrKCgpID0+IHtcbiAgICBpZiAoIWVsLl94X21vZGVsKVxuICAgICAgcmV0dXJuO1xuICAgIGVsLl94X3JlbW92ZU1vZGVsTGlzdGVuZXJzW1wiZGVmYXVsdFwiXSgpO1xuICAgIGxldCBvdXRlckdldCA9IGVsLl94X21vZGVsLmdldDtcbiAgICBsZXQgb3V0ZXJTZXQgPSBlbC5feF9tb2RlbC5zZXQ7XG4gICAgZWZmZWN0MygoKSA9PiBpbm5lclNldChvdXRlckdldCgpKSk7XG4gICAgZWZmZWN0MygoKSA9PiBvdXRlclNldChpbm5lckdldCgpKSk7XG4gIH0pO1xufSk7XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9kaXJlY3RpdmVzL3gtdGVsZXBvcnQuanNcbmRpcmVjdGl2ZShcInRlbGVwb3J0XCIsIChlbCwge2V4cHJlc3Npb259LCB7Y2xlYW51cDogY2xlYW51cDJ9KSA9PiB7XG4gIGlmIChlbC50YWdOYW1lLnRvTG93ZXJDYXNlKCkgIT09IFwidGVtcGxhdGVcIilcbiAgICB3YXJuKFwieC10ZWxlcG9ydCBjYW4gb25seSBiZSB1c2VkIG9uIGEgPHRlbXBsYXRlPiB0YWdcIiwgZWwpO1xuICBsZXQgdGFyZ2V0ID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvcihleHByZXNzaW9uKTtcbiAgaWYgKCF0YXJnZXQpXG4gICAgd2FybihgQ2Fubm90IGZpbmQgeC10ZWxlcG9ydCBlbGVtZW50IGZvciBzZWxlY3RvcjogXCIke2V4cHJlc3Npb259XCJgKTtcbiAgbGV0IGNsb25lMiA9IGVsLmNvbnRlbnQuY2xvbmVOb2RlKHRydWUpLmZpcnN0RWxlbWVudENoaWxkO1xuICBlbC5feF90ZWxlcG9ydCA9IGNsb25lMjtcbiAgY2xvbmUyLl94X3RlbGVwb3J0QmFjayA9IGVsO1xuICBpZiAoZWwuX3hfZm9yd2FyZEV2ZW50cykge1xuICAgIGVsLl94X2ZvcndhcmRFdmVudHMuZm9yRWFjaCgoZXZlbnROYW1lKSA9PiB7XG4gICAgICBjbG9uZTIuYWRkRXZlbnRMaXN0ZW5lcihldmVudE5hbWUsIChlKSA9PiB7XG4gICAgICAgIGUuc3RvcFByb3BhZ2F0aW9uKCk7XG4gICAgICAgIGVsLmRpc3BhdGNoRXZlbnQobmV3IGUuY29uc3RydWN0b3IoZS50eXBlLCBlKSk7XG4gICAgICB9KTtcbiAgICB9KTtcbiAgfVxuICBhZGRTY29wZVRvTm9kZShjbG9uZTIsIHt9LCBlbCk7XG4gIG11dGF0ZURvbSgoKSA9PiB7XG4gICAgdGFyZ2V0LmFwcGVuZENoaWxkKGNsb25lMik7XG4gICAgaW5pdFRyZWUoY2xvbmUyKTtcbiAgICBjbG9uZTIuX3hfaWdub3JlID0gdHJ1ZTtcbiAgfSk7XG4gIGNsZWFudXAyKCgpID0+IGNsb25lMi5yZW1vdmUoKSk7XG59KTtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL2RpcmVjdGl2ZXMveC1pZ25vcmUuanNcbnZhciBoYW5kbGVyID0gKCkgPT4ge1xufTtcbmhhbmRsZXIuaW5saW5lID0gKGVsLCB7bW9kaWZpZXJzfSwge2NsZWFudXA6IGNsZWFudXAyfSkgPT4ge1xuICBtb2RpZmllcnMuaW5jbHVkZXMoXCJzZWxmXCIpID8gZWwuX3hfaWdub3JlU2VsZiA9IHRydWUgOiBlbC5feF9pZ25vcmUgPSB0cnVlO1xuICBjbGVhbnVwMigoKSA9PiB7XG4gICAgbW9kaWZpZXJzLmluY2x1ZGVzKFwic2VsZlwiKSA/IGRlbGV0ZSBlbC5feF9pZ25vcmVTZWxmIDogZGVsZXRlIGVsLl94X2lnbm9yZTtcbiAgfSk7XG59O1xuZGlyZWN0aXZlKFwiaWdub3JlXCIsIGhhbmRsZXIpO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy94LWVmZmVjdC5qc1xuZGlyZWN0aXZlKFwiZWZmZWN0XCIsIChlbCwge2V4cHJlc3Npb259LCB7ZWZmZWN0OiBlZmZlY3QzfSkgPT4gZWZmZWN0MyhldmFsdWF0ZUxhdGVyKGVsLCBleHByZXNzaW9uKSkpO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvdXRpbHMvb24uanNcbmZ1bmN0aW9uIG9uKGVsLCBldmVudCwgbW9kaWZpZXJzLCBjYWxsYmFjaykge1xuICBsZXQgbGlzdGVuZXJUYXJnZXQgPSBlbDtcbiAgbGV0IGhhbmRsZXIzID0gKGUpID0+IGNhbGxiYWNrKGUpO1xuICBsZXQgb3B0aW9ucyA9IHt9O1xuICBsZXQgd3JhcEhhbmRsZXIgPSAoY2FsbGJhY2syLCB3cmFwcGVyKSA9PiAoZSkgPT4gd3JhcHBlcihjYWxsYmFjazIsIGUpO1xuICBpZiAobW9kaWZpZXJzLmluY2x1ZGVzKFwiZG90XCIpKVxuICAgIGV2ZW50ID0gZG90U3ludGF4KGV2ZW50KTtcbiAgaWYgKG1vZGlmaWVycy5pbmNsdWRlcyhcImNhbWVsXCIpKVxuICAgIGV2ZW50ID0gY2FtZWxDYXNlMihldmVudCk7XG4gIGlmIChtb2RpZmllcnMuaW5jbHVkZXMoXCJwYXNzaXZlXCIpKVxuICAgIG9wdGlvbnMucGFzc2l2ZSA9IHRydWU7XG4gIGlmIChtb2RpZmllcnMuaW5jbHVkZXMoXCJjYXB0dXJlXCIpKVxuICAgIG9wdGlvbnMuY2FwdHVyZSA9IHRydWU7XG4gIGlmIChtb2RpZmllcnMuaW5jbHVkZXMoXCJ3aW5kb3dcIikpXG4gICAgbGlzdGVuZXJUYXJnZXQgPSB3aW5kb3c7XG4gIGlmIChtb2RpZmllcnMuaW5jbHVkZXMoXCJkb2N1bWVudFwiKSlcbiAgICBsaXN0ZW5lclRhcmdldCA9IGRvY3VtZW50O1xuICBpZiAobW9kaWZpZXJzLmluY2x1ZGVzKFwicHJldmVudFwiKSlcbiAgICBoYW5kbGVyMyA9IHdyYXBIYW5kbGVyKGhhbmRsZXIzLCAobmV4dCwgZSkgPT4ge1xuICAgICAgZS5wcmV2ZW50RGVmYXVsdCgpO1xuICAgICAgbmV4dChlKTtcbiAgICB9KTtcbiAgaWYgKG1vZGlmaWVycy5pbmNsdWRlcyhcInN0b3BcIikpXG4gICAgaGFuZGxlcjMgPSB3cmFwSGFuZGxlcihoYW5kbGVyMywgKG5leHQsIGUpID0+IHtcbiAgICAgIGUuc3RvcFByb3BhZ2F0aW9uKCk7XG4gICAgICBuZXh0KGUpO1xuICAgIH0pO1xuICBpZiAobW9kaWZpZXJzLmluY2x1ZGVzKFwic2VsZlwiKSlcbiAgICBoYW5kbGVyMyA9IHdyYXBIYW5kbGVyKGhhbmRsZXIzLCAobmV4dCwgZSkgPT4ge1xuICAgICAgZS50YXJnZXQgPT09IGVsICYmIG5leHQoZSk7XG4gICAgfSk7XG4gIGlmIChtb2RpZmllcnMuaW5jbHVkZXMoXCJhd2F5XCIpIHx8IG1vZGlmaWVycy5pbmNsdWRlcyhcIm91dHNpZGVcIikpIHtcbiAgICBsaXN0ZW5lclRhcmdldCA9IGRvY3VtZW50O1xuICAgIGhhbmRsZXIzID0gd3JhcEhhbmRsZXIoaGFuZGxlcjMsIChuZXh0LCBlKSA9PiB7XG4gICAgICBpZiAoZWwuY29udGFpbnMoZS50YXJnZXQpKVxuICAgICAgICByZXR1cm47XG4gICAgICBpZiAoZS50YXJnZXQuaXNDb25uZWN0ZWQgPT09IGZhbHNlKVxuICAgICAgICByZXR1cm47XG4gICAgICBpZiAoZWwub2Zmc2V0V2lkdGggPCAxICYmIGVsLm9mZnNldEhlaWdodCA8IDEpXG4gICAgICAgIHJldHVybjtcbiAgICAgIGlmIChlbC5feF9pc1Nob3duID09PSBmYWxzZSlcbiAgICAgICAgcmV0dXJuO1xuICAgICAgbmV4dChlKTtcbiAgICB9KTtcbiAgfVxuICBpZiAobW9kaWZpZXJzLmluY2x1ZGVzKFwib25jZVwiKSkge1xuICAgIGhhbmRsZXIzID0gd3JhcEhhbmRsZXIoaGFuZGxlcjMsIChuZXh0LCBlKSA9PiB7XG4gICAgICBuZXh0KGUpO1xuICAgICAgbGlzdGVuZXJUYXJnZXQucmVtb3ZlRXZlbnRMaXN0ZW5lcihldmVudCwgaGFuZGxlcjMsIG9wdGlvbnMpO1xuICAgIH0pO1xuICB9XG4gIGhhbmRsZXIzID0gd3JhcEhhbmRsZXIoaGFuZGxlcjMsIChuZXh0LCBlKSA9PiB7XG4gICAgaWYgKGlzS2V5RXZlbnQoZXZlbnQpKSB7XG4gICAgICBpZiAoaXNMaXN0ZW5pbmdGb3JBU3BlY2lmaWNLZXlUaGF0SGFzbnRCZWVuUHJlc3NlZChlLCBtb2RpZmllcnMpKSB7XG4gICAgICAgIHJldHVybjtcbiAgICAgIH1cbiAgICB9XG4gICAgbmV4dChlKTtcbiAgfSk7XG4gIGlmIChtb2RpZmllcnMuaW5jbHVkZXMoXCJkZWJvdW5jZVwiKSkge1xuICAgIGxldCBuZXh0TW9kaWZpZXIgPSBtb2RpZmllcnNbbW9kaWZpZXJzLmluZGV4T2YoXCJkZWJvdW5jZVwiKSArIDFdIHx8IFwiaW52YWxpZC13YWl0XCI7XG4gICAgbGV0IHdhaXQgPSBpc051bWVyaWMobmV4dE1vZGlmaWVyLnNwbGl0KFwibXNcIilbMF0pID8gTnVtYmVyKG5leHRNb2RpZmllci5zcGxpdChcIm1zXCIpWzBdKSA6IDI1MDtcbiAgICBoYW5kbGVyMyA9IGRlYm91bmNlKGhhbmRsZXIzLCB3YWl0KTtcbiAgfVxuICBpZiAobW9kaWZpZXJzLmluY2x1ZGVzKFwidGhyb3R0bGVcIikpIHtcbiAgICBsZXQgbmV4dE1vZGlmaWVyID0gbW9kaWZpZXJzW21vZGlmaWVycy5pbmRleE9mKFwidGhyb3R0bGVcIikgKyAxXSB8fCBcImludmFsaWQtd2FpdFwiO1xuICAgIGxldCB3YWl0ID0gaXNOdW1lcmljKG5leHRNb2RpZmllci5zcGxpdChcIm1zXCIpWzBdKSA/IE51bWJlcihuZXh0TW9kaWZpZXIuc3BsaXQoXCJtc1wiKVswXSkgOiAyNTA7XG4gICAgaGFuZGxlcjMgPSB0aHJvdHRsZShoYW5kbGVyMywgd2FpdCk7XG4gIH1cbiAgbGlzdGVuZXJUYXJnZXQuYWRkRXZlbnRMaXN0ZW5lcihldmVudCwgaGFuZGxlcjMsIG9wdGlvbnMpO1xuICByZXR1cm4gKCkgPT4ge1xuICAgIGxpc3RlbmVyVGFyZ2V0LnJlbW92ZUV2ZW50TGlzdGVuZXIoZXZlbnQsIGhhbmRsZXIzLCBvcHRpb25zKTtcbiAgfTtcbn1cbmZ1bmN0aW9uIGRvdFN5bnRheChzdWJqZWN0KSB7XG4gIHJldHVybiBzdWJqZWN0LnJlcGxhY2UoLy0vZywgXCIuXCIpO1xufVxuZnVuY3Rpb24gY2FtZWxDYXNlMihzdWJqZWN0KSB7XG4gIHJldHVybiBzdWJqZWN0LnRvTG93ZXJDYXNlKCkucmVwbGFjZSgvLShcXHcpL2csIChtYXRjaCwgY2hhcikgPT4gY2hhci50b1VwcGVyQ2FzZSgpKTtcbn1cbmZ1bmN0aW9uIGlzTnVtZXJpYyhzdWJqZWN0KSB7XG4gIHJldHVybiAhQXJyYXkuaXNBcnJheShzdWJqZWN0KSAmJiAhaXNOYU4oc3ViamVjdCk7XG59XG5mdW5jdGlvbiBrZWJhYkNhc2UyKHN1YmplY3QpIHtcbiAgcmV0dXJuIHN1YmplY3QucmVwbGFjZSgvKFthLXpdKShbQS1aXSkvZywgXCIkMS0kMlwiKS5yZXBsYWNlKC9bX1xcc10vLCBcIi1cIikudG9Mb3dlckNhc2UoKTtcbn1cbmZ1bmN0aW9uIGlzS2V5RXZlbnQoZXZlbnQpIHtcbiAgcmV0dXJuIFtcImtleWRvd25cIiwgXCJrZXl1cFwiXS5pbmNsdWRlcyhldmVudCk7XG59XG5mdW5jdGlvbiBpc0xpc3RlbmluZ0ZvckFTcGVjaWZpY0tleVRoYXRIYXNudEJlZW5QcmVzc2VkKGUsIG1vZGlmaWVycykge1xuICBsZXQga2V5TW9kaWZpZXJzID0gbW9kaWZpZXJzLmZpbHRlcigoaSkgPT4ge1xuICAgIHJldHVybiAhW1wid2luZG93XCIsIFwiZG9jdW1lbnRcIiwgXCJwcmV2ZW50XCIsIFwic3RvcFwiLCBcIm9uY2VcIl0uaW5jbHVkZXMoaSk7XG4gIH0pO1xuICBpZiAoa2V5TW9kaWZpZXJzLmluY2x1ZGVzKFwiZGVib3VuY2VcIikpIHtcbiAgICBsZXQgZGVib3VuY2VJbmRleCA9IGtleU1vZGlmaWVycy5pbmRleE9mKFwiZGVib3VuY2VcIik7XG4gICAga2V5TW9kaWZpZXJzLnNwbGljZShkZWJvdW5jZUluZGV4LCBpc051bWVyaWMoKGtleU1vZGlmaWVyc1tkZWJvdW5jZUluZGV4ICsgMV0gfHwgXCJpbnZhbGlkLXdhaXRcIikuc3BsaXQoXCJtc1wiKVswXSkgPyAyIDogMSk7XG4gIH1cbiAgaWYgKGtleU1vZGlmaWVycy5sZW5ndGggPT09IDApXG4gICAgcmV0dXJuIGZhbHNlO1xuICBpZiAoa2V5TW9kaWZpZXJzLmxlbmd0aCA9PT0gMSAmJiBrZXlUb01vZGlmaWVycyhlLmtleSkuaW5jbHVkZXMoa2V5TW9kaWZpZXJzWzBdKSlcbiAgICByZXR1cm4gZmFsc2U7XG4gIGNvbnN0IHN5c3RlbUtleU1vZGlmaWVycyA9IFtcImN0cmxcIiwgXCJzaGlmdFwiLCBcImFsdFwiLCBcIm1ldGFcIiwgXCJjbWRcIiwgXCJzdXBlclwiXTtcbiAgY29uc3Qgc2VsZWN0ZWRTeXN0ZW1LZXlNb2RpZmllcnMgPSBzeXN0ZW1LZXlNb2RpZmllcnMuZmlsdGVyKChtb2RpZmllcikgPT4ga2V5TW9kaWZpZXJzLmluY2x1ZGVzKG1vZGlmaWVyKSk7XG4gIGtleU1vZGlmaWVycyA9IGtleU1vZGlmaWVycy5maWx0ZXIoKGkpID0+ICFzZWxlY3RlZFN5c3RlbUtleU1vZGlmaWVycy5pbmNsdWRlcyhpKSk7XG4gIGlmIChzZWxlY3RlZFN5c3RlbUtleU1vZGlmaWVycy5sZW5ndGggPiAwKSB7XG4gICAgY29uc3QgYWN0aXZlbHlQcmVzc2VkS2V5TW9kaWZpZXJzID0gc2VsZWN0ZWRTeXN0ZW1LZXlNb2RpZmllcnMuZmlsdGVyKChtb2RpZmllcikgPT4ge1xuICAgICAgaWYgKG1vZGlmaWVyID09PSBcImNtZFwiIHx8IG1vZGlmaWVyID09PSBcInN1cGVyXCIpXG4gICAgICAgIG1vZGlmaWVyID0gXCJtZXRhXCI7XG4gICAgICByZXR1cm4gZVtgJHttb2RpZmllcn1LZXlgXTtcbiAgICB9KTtcbiAgICBpZiAoYWN0aXZlbHlQcmVzc2VkS2V5TW9kaWZpZXJzLmxlbmd0aCA9PT0gc2VsZWN0ZWRTeXN0ZW1LZXlNb2RpZmllcnMubGVuZ3RoKSB7XG4gICAgICBpZiAoa2V5VG9Nb2RpZmllcnMoZS5rZXkpLmluY2x1ZGVzKGtleU1vZGlmaWVyc1swXSkpXG4gICAgICAgIHJldHVybiBmYWxzZTtcbiAgICB9XG4gIH1cbiAgcmV0dXJuIHRydWU7XG59XG5mdW5jdGlvbiBrZXlUb01vZGlmaWVycyhrZXkpIHtcbiAgaWYgKCFrZXkpXG4gICAgcmV0dXJuIFtdO1xuICBrZXkgPSBrZWJhYkNhc2UyKGtleSk7XG4gIGxldCBtb2RpZmllclRvS2V5TWFwID0ge1xuICAgIGN0cmw6IFwiY29udHJvbFwiLFxuICAgIHNsYXNoOiBcIi9cIixcbiAgICBzcGFjZTogXCItXCIsXG4gICAgc3BhY2ViYXI6IFwiLVwiLFxuICAgIGNtZDogXCJtZXRhXCIsXG4gICAgZXNjOiBcImVzY2FwZVwiLFxuICAgIHVwOiBcImFycm93LXVwXCIsXG4gICAgZG93bjogXCJhcnJvdy1kb3duXCIsXG4gICAgbGVmdDogXCJhcnJvdy1sZWZ0XCIsXG4gICAgcmlnaHQ6IFwiYXJyb3ctcmlnaHRcIixcbiAgICBwZXJpb2Q6IFwiLlwiLFxuICAgIGVxdWFsOiBcIj1cIlxuICB9O1xuICBtb2RpZmllclRvS2V5TWFwW2tleV0gPSBrZXk7XG4gIHJldHVybiBPYmplY3Qua2V5cyhtb2RpZmllclRvS2V5TWFwKS5tYXAoKG1vZGlmaWVyKSA9PiB7XG4gICAgaWYgKG1vZGlmaWVyVG9LZXlNYXBbbW9kaWZpZXJdID09PSBrZXkpXG4gICAgICByZXR1cm4gbW9kaWZpZXI7XG4gIH0pLmZpbHRlcigobW9kaWZpZXIpID0+IG1vZGlmaWVyKTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL2RpcmVjdGl2ZXMveC1tb2RlbC5qc1xuZGlyZWN0aXZlKFwibW9kZWxcIiwgKGVsLCB7bW9kaWZpZXJzLCBleHByZXNzaW9ufSwge2VmZmVjdDogZWZmZWN0MywgY2xlYW51cDogY2xlYW51cDJ9KSA9PiB7XG4gIGxldCBldmFsdWF0ZTIgPSBldmFsdWF0ZUxhdGVyKGVsLCBleHByZXNzaW9uKTtcbiAgbGV0IGFzc2lnbm1lbnRFeHByZXNzaW9uID0gYCR7ZXhwcmVzc2lvbn0gPSByaWdodFNpZGVPZkV4cHJlc3Npb24oJGV2ZW50LCAke2V4cHJlc3Npb259KWA7XG4gIGxldCBldmFsdWF0ZUFzc2lnbm1lbnQgPSBldmFsdWF0ZUxhdGVyKGVsLCBhc3NpZ25tZW50RXhwcmVzc2lvbik7XG4gIHZhciBldmVudCA9IGVsLnRhZ05hbWUudG9Mb3dlckNhc2UoKSA9PT0gXCJzZWxlY3RcIiB8fCBbXCJjaGVja2JveFwiLCBcInJhZGlvXCJdLmluY2x1ZGVzKGVsLnR5cGUpIHx8IG1vZGlmaWVycy5pbmNsdWRlcyhcImxhenlcIikgPyBcImNoYW5nZVwiIDogXCJpbnB1dFwiO1xuICBsZXQgYXNzaWdtZW50RnVuY3Rpb24gPSBnZW5lcmF0ZUFzc2lnbm1lbnRGdW5jdGlvbihlbCwgbW9kaWZpZXJzLCBleHByZXNzaW9uKTtcbiAgbGV0IHJlbW92ZUxpc3RlbmVyID0gb24oZWwsIGV2ZW50LCBtb2RpZmllcnMsIChlKSA9PiB7XG4gICAgZXZhbHVhdGVBc3NpZ25tZW50KCgpID0+IHtcbiAgICB9LCB7c2NvcGU6IHtcbiAgICAgICRldmVudDogZSxcbiAgICAgIHJpZ2h0U2lkZU9mRXhwcmVzc2lvbjogYXNzaWdtZW50RnVuY3Rpb25cbiAgICB9fSk7XG4gIH0pO1xuICBpZiAoIWVsLl94X3JlbW92ZU1vZGVsTGlzdGVuZXJzKVxuICAgIGVsLl94X3JlbW92ZU1vZGVsTGlzdGVuZXJzID0ge307XG4gIGVsLl94X3JlbW92ZU1vZGVsTGlzdGVuZXJzW1wiZGVmYXVsdFwiXSA9IHJlbW92ZUxpc3RlbmVyO1xuICBjbGVhbnVwMigoKSA9PiBlbC5feF9yZW1vdmVNb2RlbExpc3RlbmVyc1tcImRlZmF1bHRcIl0oKSk7XG4gIGxldCBldmFsdWF0ZVNldE1vZGVsID0gZXZhbHVhdGVMYXRlcihlbCwgYCR7ZXhwcmVzc2lvbn0gPSBfX3BsYWNlaG9sZGVyYCk7XG4gIGVsLl94X21vZGVsID0ge1xuICAgIGdldCgpIHtcbiAgICAgIGxldCByZXN1bHQ7XG4gICAgICBldmFsdWF0ZTIoKHZhbHVlKSA9PiByZXN1bHQgPSB2YWx1ZSk7XG4gICAgICByZXR1cm4gcmVzdWx0O1xuICAgIH0sXG4gICAgc2V0KHZhbHVlKSB7XG4gICAgICBldmFsdWF0ZVNldE1vZGVsKCgpID0+IHtcbiAgICAgIH0sIHtzY29wZToge19fcGxhY2Vob2xkZXI6IHZhbHVlfX0pO1xuICAgIH1cbiAgfTtcbiAgZWwuX3hfZm9yY2VNb2RlbFVwZGF0ZSA9ICgpID0+IHtcbiAgICBldmFsdWF0ZTIoKHZhbHVlKSA9PiB7XG4gICAgICBpZiAodmFsdWUgPT09IHZvaWQgMCAmJiBleHByZXNzaW9uLm1hdGNoKC9cXC4vKSlcbiAgICAgICAgdmFsdWUgPSBcIlwiO1xuICAgICAgd2luZG93LmZyb21Nb2RlbCA9IHRydWU7XG4gICAgICBtdXRhdGVEb20oKCkgPT4gYmluZChlbCwgXCJ2YWx1ZVwiLCB2YWx1ZSkpO1xuICAgICAgZGVsZXRlIHdpbmRvdy5mcm9tTW9kZWw7XG4gICAgfSk7XG4gIH07XG4gIGVmZmVjdDMoKCkgPT4ge1xuICAgIGlmIChtb2RpZmllcnMuaW5jbHVkZXMoXCJ1bmludHJ1c2l2ZVwiKSAmJiBkb2N1bWVudC5hY3RpdmVFbGVtZW50LmlzU2FtZU5vZGUoZWwpKVxuICAgICAgcmV0dXJuO1xuICAgIGVsLl94X2ZvcmNlTW9kZWxVcGRhdGUoKTtcbiAgfSk7XG59KTtcbmZ1bmN0aW9uIGdlbmVyYXRlQXNzaWdubWVudEZ1bmN0aW9uKGVsLCBtb2RpZmllcnMsIGV4cHJlc3Npb24pIHtcbiAgaWYgKGVsLnR5cGUgPT09IFwicmFkaW9cIikge1xuICAgIG11dGF0ZURvbSgoKSA9PiB7XG4gICAgICBpZiAoIWVsLmhhc0F0dHJpYnV0ZShcIm5hbWVcIikpXG4gICAgICAgIGVsLnNldEF0dHJpYnV0ZShcIm5hbWVcIiwgZXhwcmVzc2lvbik7XG4gICAgfSk7XG4gIH1cbiAgcmV0dXJuIChldmVudCwgY3VycmVudFZhbHVlKSA9PiB7XG4gICAgcmV0dXJuIG11dGF0ZURvbSgoKSA9PiB7XG4gICAgICBpZiAoZXZlbnQgaW5zdGFuY2VvZiBDdXN0b21FdmVudCAmJiBldmVudC5kZXRhaWwgIT09IHZvaWQgMCkge1xuICAgICAgICByZXR1cm4gZXZlbnQuZGV0YWlsIHx8IGV2ZW50LnRhcmdldC52YWx1ZTtcbiAgICAgIH0gZWxzZSBpZiAoZWwudHlwZSA9PT0gXCJjaGVja2JveFwiKSB7XG4gICAgICAgIGlmIChBcnJheS5pc0FycmF5KGN1cnJlbnRWYWx1ZSkpIHtcbiAgICAgICAgICBsZXQgbmV3VmFsdWUgPSBtb2RpZmllcnMuaW5jbHVkZXMoXCJudW1iZXJcIikgPyBzYWZlUGFyc2VOdW1iZXIoZXZlbnQudGFyZ2V0LnZhbHVlKSA6IGV2ZW50LnRhcmdldC52YWx1ZTtcbiAgICAgICAgICByZXR1cm4gZXZlbnQudGFyZ2V0LmNoZWNrZWQgPyBjdXJyZW50VmFsdWUuY29uY2F0KFtuZXdWYWx1ZV0pIDogY3VycmVudFZhbHVlLmZpbHRlcigoZWwyKSA9PiAhY2hlY2tlZEF0dHJMb29zZUNvbXBhcmUyKGVsMiwgbmV3VmFsdWUpKTtcbiAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICByZXR1cm4gZXZlbnQudGFyZ2V0LmNoZWNrZWQ7XG4gICAgICAgIH1cbiAgICAgIH0gZWxzZSBpZiAoZWwudGFnTmFtZS50b0xvd2VyQ2FzZSgpID09PSBcInNlbGVjdFwiICYmIGVsLm11bHRpcGxlKSB7XG4gICAgICAgIHJldHVybiBtb2RpZmllcnMuaW5jbHVkZXMoXCJudW1iZXJcIikgPyBBcnJheS5mcm9tKGV2ZW50LnRhcmdldC5zZWxlY3RlZE9wdGlvbnMpLm1hcCgob3B0aW9uKSA9PiB7XG4gICAgICAgICAgbGV0IHJhd1ZhbHVlID0gb3B0aW9uLnZhbHVlIHx8IG9wdGlvbi50ZXh0O1xuICAgICAgICAgIHJldHVybiBzYWZlUGFyc2VOdW1iZXIocmF3VmFsdWUpO1xuICAgICAgICB9KSA6IEFycmF5LmZyb20oZXZlbnQudGFyZ2V0LnNlbGVjdGVkT3B0aW9ucykubWFwKChvcHRpb24pID0+IHtcbiAgICAgICAgICByZXR1cm4gb3B0aW9uLnZhbHVlIHx8IG9wdGlvbi50ZXh0O1xuICAgICAgICB9KTtcbiAgICAgIH0gZWxzZSB7XG4gICAgICAgIGxldCByYXdWYWx1ZSA9IGV2ZW50LnRhcmdldC52YWx1ZTtcbiAgICAgICAgcmV0dXJuIG1vZGlmaWVycy5pbmNsdWRlcyhcIm51bWJlclwiKSA/IHNhZmVQYXJzZU51bWJlcihyYXdWYWx1ZSkgOiBtb2RpZmllcnMuaW5jbHVkZXMoXCJ0cmltXCIpID8gcmF3VmFsdWUudHJpbSgpIDogcmF3VmFsdWU7XG4gICAgICB9XG4gICAgfSk7XG4gIH07XG59XG5mdW5jdGlvbiBzYWZlUGFyc2VOdW1iZXIocmF3VmFsdWUpIHtcbiAgbGV0IG51bWJlciA9IHJhd1ZhbHVlID8gcGFyc2VGbG9hdChyYXdWYWx1ZSkgOiBudWxsO1xuICByZXR1cm4gaXNOdW1lcmljMihudW1iZXIpID8gbnVtYmVyIDogcmF3VmFsdWU7XG59XG5mdW5jdGlvbiBjaGVja2VkQXR0ckxvb3NlQ29tcGFyZTIodmFsdWVBLCB2YWx1ZUIpIHtcbiAgcmV0dXJuIHZhbHVlQSA9PSB2YWx1ZUI7XG59XG5mdW5jdGlvbiBpc051bWVyaWMyKHN1YmplY3QpIHtcbiAgcmV0dXJuICFBcnJheS5pc0FycmF5KHN1YmplY3QpICYmICFpc05hTihzdWJqZWN0KTtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL2RpcmVjdGl2ZXMveC1jbG9hay5qc1xuZGlyZWN0aXZlKFwiY2xvYWtcIiwgKGVsKSA9PiBxdWV1ZU1pY3JvdGFzaygoKSA9PiBtdXRhdGVEb20oKCkgPT4gZWwucmVtb3ZlQXR0cmlidXRlKHByZWZpeChcImNsb2FrXCIpKSkpKTtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL2RpcmVjdGl2ZXMveC1pbml0LmpzXG5hZGRJbml0U2VsZWN0b3IoKCkgPT4gYFske3ByZWZpeChcImluaXRcIil9XWApO1xuZGlyZWN0aXZlKFwiaW5pdFwiLCBza2lwRHVyaW5nQ2xvbmUoKGVsLCB7ZXhwcmVzc2lvbn0sIHtldmFsdWF0ZTogZXZhbHVhdGUyfSkgPT4ge1xuICBpZiAodHlwZW9mIGV4cHJlc3Npb24gPT09IFwic3RyaW5nXCIpIHtcbiAgICByZXR1cm4gISFleHByZXNzaW9uLnRyaW0oKSAmJiBldmFsdWF0ZTIoZXhwcmVzc2lvbiwge30sIGZhbHNlKTtcbiAgfVxuICByZXR1cm4gZXZhbHVhdGUyKGV4cHJlc3Npb24sIHt9LCBmYWxzZSk7XG59KSk7XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9kaXJlY3RpdmVzL3gtdGV4dC5qc1xuZGlyZWN0aXZlKFwidGV4dFwiLCAoZWwsIHtleHByZXNzaW9ufSwge2VmZmVjdDogZWZmZWN0MywgZXZhbHVhdGVMYXRlcjogZXZhbHVhdGVMYXRlcjJ9KSA9PiB7XG4gIGxldCBldmFsdWF0ZTIgPSBldmFsdWF0ZUxhdGVyMihleHByZXNzaW9uKTtcbiAgZWZmZWN0MygoKSA9PiB7XG4gICAgZXZhbHVhdGUyKCh2YWx1ZSkgPT4ge1xuICAgICAgbXV0YXRlRG9tKCgpID0+IHtcbiAgICAgICAgZWwudGV4dENvbnRlbnQgPSB2YWx1ZTtcbiAgICAgIH0pO1xuICAgIH0pO1xuICB9KTtcbn0pO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy94LWh0bWwuanNcbmRpcmVjdGl2ZShcImh0bWxcIiwgKGVsLCB7ZXhwcmVzc2lvbn0sIHtlZmZlY3Q6IGVmZmVjdDMsIGV2YWx1YXRlTGF0ZXI6IGV2YWx1YXRlTGF0ZXIyfSkgPT4ge1xuICBsZXQgZXZhbHVhdGUyID0gZXZhbHVhdGVMYXRlcjIoZXhwcmVzc2lvbik7XG4gIGVmZmVjdDMoKCkgPT4ge1xuICAgIGV2YWx1YXRlMigodmFsdWUpID0+IHtcbiAgICAgIG11dGF0ZURvbSgoKSA9PiB7XG4gICAgICAgIGVsLmlubmVySFRNTCA9IHZhbHVlO1xuICAgICAgICBlbC5feF9pZ25vcmVTZWxmID0gdHJ1ZTtcbiAgICAgICAgaW5pdFRyZWUoZWwpO1xuICAgICAgICBkZWxldGUgZWwuX3hfaWdub3JlU2VsZjtcbiAgICAgIH0pO1xuICAgIH0pO1xuICB9KTtcbn0pO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy94LWJpbmQuanNcbm1hcEF0dHJpYnV0ZXMoc3RhcnRpbmdXaXRoKFwiOlwiLCBpbnRvKHByZWZpeChcImJpbmQ6XCIpKSkpO1xuZGlyZWN0aXZlKFwiYmluZFwiLCAoZWwsIHt2YWx1ZSwgbW9kaWZpZXJzLCBleHByZXNzaW9uLCBvcmlnaW5hbH0sIHtlZmZlY3Q6IGVmZmVjdDN9KSA9PiB7XG4gIGlmICghdmFsdWUpIHtcbiAgICBsZXQgYmluZGluZ1Byb3ZpZGVycyA9IHt9O1xuICAgIGluamVjdEJpbmRpbmdQcm92aWRlcnMoYmluZGluZ1Byb3ZpZGVycyk7XG4gICAgbGV0IGdldEJpbmRpbmdzID0gZXZhbHVhdGVMYXRlcihlbCwgZXhwcmVzc2lvbik7XG4gICAgZ2V0QmluZGluZ3MoKGJpbmRpbmdzKSA9PiB7XG4gICAgICBhcHBseUJpbmRpbmdzT2JqZWN0KGVsLCBiaW5kaW5ncywgb3JpZ2luYWwpO1xuICAgIH0sIHtzY29wZTogYmluZGluZ1Byb3ZpZGVyc30pO1xuICAgIHJldHVybjtcbiAgfVxuICBpZiAodmFsdWUgPT09IFwia2V5XCIpXG4gICAgcmV0dXJuIHN0b3JlS2V5Rm9yWEZvcihlbCwgZXhwcmVzc2lvbik7XG4gIGxldCBldmFsdWF0ZTIgPSBldmFsdWF0ZUxhdGVyKGVsLCBleHByZXNzaW9uKTtcbiAgZWZmZWN0MygoKSA9PiBldmFsdWF0ZTIoKHJlc3VsdCkgPT4ge1xuICAgIGlmIChyZXN1bHQgPT09IHZvaWQgMCAmJiB0eXBlb2YgZXhwcmVzc2lvbiA9PT0gXCJzdHJpbmdcIiAmJiBleHByZXNzaW9uLm1hdGNoKC9cXC4vKSkge1xuICAgICAgcmVzdWx0ID0gXCJcIjtcbiAgICB9XG4gICAgbXV0YXRlRG9tKCgpID0+IGJpbmQoZWwsIHZhbHVlLCByZXN1bHQsIG1vZGlmaWVycykpO1xuICB9KSk7XG59KTtcbmZ1bmN0aW9uIHN0b3JlS2V5Rm9yWEZvcihlbCwgZXhwcmVzc2lvbikge1xuICBlbC5feF9rZXlFeHByZXNzaW9uID0gZXhwcmVzc2lvbjtcbn1cblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL2RpcmVjdGl2ZXMveC1kYXRhLmpzXG5hZGRSb290U2VsZWN0b3IoKCkgPT4gYFske3ByZWZpeChcImRhdGFcIil9XWApO1xuZGlyZWN0aXZlKFwiZGF0YVwiLCBza2lwRHVyaW5nQ2xvbmUoKGVsLCB7ZXhwcmVzc2lvbn0sIHtjbGVhbnVwOiBjbGVhbnVwMn0pID0+IHtcbiAgZXhwcmVzc2lvbiA9IGV4cHJlc3Npb24gPT09IFwiXCIgPyBcInt9XCIgOiBleHByZXNzaW9uO1xuICBsZXQgbWFnaWNDb250ZXh0ID0ge307XG4gIGluamVjdE1hZ2ljcyhtYWdpY0NvbnRleHQsIGVsKTtcbiAgbGV0IGRhdGFQcm92aWRlckNvbnRleHQgPSB7fTtcbiAgaW5qZWN0RGF0YVByb3ZpZGVycyhkYXRhUHJvdmlkZXJDb250ZXh0LCBtYWdpY0NvbnRleHQpO1xuICBsZXQgZGF0YTIgPSBldmFsdWF0ZShlbCwgZXhwcmVzc2lvbiwge3Njb3BlOiBkYXRhUHJvdmlkZXJDb250ZXh0fSk7XG4gIGlmIChkYXRhMiA9PT0gdm9pZCAwKVxuICAgIGRhdGEyID0ge307XG4gIGluamVjdE1hZ2ljcyhkYXRhMiwgZWwpO1xuICBsZXQgcmVhY3RpdmVEYXRhID0gcmVhY3RpdmUoZGF0YTIpO1xuICBpbml0SW50ZXJjZXB0b3JzKHJlYWN0aXZlRGF0YSk7XG4gIGxldCB1bmRvID0gYWRkU2NvcGVUb05vZGUoZWwsIHJlYWN0aXZlRGF0YSk7XG4gIHJlYWN0aXZlRGF0YVtcImluaXRcIl0gJiYgZXZhbHVhdGUoZWwsIHJlYWN0aXZlRGF0YVtcImluaXRcIl0pO1xuICBjbGVhbnVwMigoKSA9PiB7XG4gICAgcmVhY3RpdmVEYXRhW1wiZGVzdHJveVwiXSAmJiBldmFsdWF0ZShlbCwgcmVhY3RpdmVEYXRhW1wiZGVzdHJveVwiXSk7XG4gICAgdW5kbygpO1xuICB9KTtcbn0pKTtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvc3JjL2RpcmVjdGl2ZXMveC1zaG93LmpzXG5kaXJlY3RpdmUoXCJzaG93XCIsIChlbCwge21vZGlmaWVycywgZXhwcmVzc2lvbn0sIHtlZmZlY3Q6IGVmZmVjdDN9KSA9PiB7XG4gIGxldCBldmFsdWF0ZTIgPSBldmFsdWF0ZUxhdGVyKGVsLCBleHByZXNzaW9uKTtcbiAgaWYgKCFlbC5feF9kb0hpZGUpXG4gICAgZWwuX3hfZG9IaWRlID0gKCkgPT4ge1xuICAgICAgbXV0YXRlRG9tKCgpID0+IHtcbiAgICAgICAgZWwuc3R5bGUuc2V0UHJvcGVydHkoXCJkaXNwbGF5XCIsIFwibm9uZVwiLCBtb2RpZmllcnMuaW5jbHVkZXMoXCJpbXBvcnRhbnRcIikgPyBcImltcG9ydGFudFwiIDogdm9pZCAwKTtcbiAgICAgIH0pO1xuICAgIH07XG4gIGlmICghZWwuX3hfZG9TaG93KVxuICAgIGVsLl94X2RvU2hvdyA9ICgpID0+IHtcbiAgICAgIG11dGF0ZURvbSgoKSA9PiB7XG4gICAgICAgIGlmIChlbC5zdHlsZS5sZW5ndGggPT09IDEgJiYgZWwuc3R5bGUuZGlzcGxheSA9PT0gXCJub25lXCIpIHtcbiAgICAgICAgICBlbC5yZW1vdmVBdHRyaWJ1dGUoXCJzdHlsZVwiKTtcbiAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICBlbC5zdHlsZS5yZW1vdmVQcm9wZXJ0eShcImRpc3BsYXlcIik7XG4gICAgICAgIH1cbiAgICAgIH0pO1xuICAgIH07XG4gIGxldCBoaWRlID0gKCkgPT4ge1xuICAgIGVsLl94X2RvSGlkZSgpO1xuICAgIGVsLl94X2lzU2hvd24gPSBmYWxzZTtcbiAgfTtcbiAgbGV0IHNob3cgPSAoKSA9PiB7XG4gICAgZWwuX3hfZG9TaG93KCk7XG4gICAgZWwuX3hfaXNTaG93biA9IHRydWU7XG4gIH07XG4gIGxldCBjbGlja0F3YXlDb21wYXRpYmxlU2hvdyA9ICgpID0+IHNldFRpbWVvdXQoc2hvdyk7XG4gIGxldCB0b2dnbGUgPSBvbmNlKCh2YWx1ZSkgPT4gdmFsdWUgPyBzaG93KCkgOiBoaWRlKCksICh2YWx1ZSkgPT4ge1xuICAgIGlmICh0eXBlb2YgZWwuX3hfdG9nZ2xlQW5kQ2FzY2FkZVdpdGhUcmFuc2l0aW9ucyA9PT0gXCJmdW5jdGlvblwiKSB7XG4gICAgICBlbC5feF90b2dnbGVBbmRDYXNjYWRlV2l0aFRyYW5zaXRpb25zKGVsLCB2YWx1ZSwgc2hvdywgaGlkZSk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHZhbHVlID8gY2xpY2tBd2F5Q29tcGF0aWJsZVNob3coKSA6IGhpZGUoKTtcbiAgICB9XG4gIH0pO1xuICBsZXQgb2xkVmFsdWU7XG4gIGxldCBmaXJzdFRpbWUgPSB0cnVlO1xuICBlZmZlY3QzKCgpID0+IGV2YWx1YXRlMigodmFsdWUpID0+IHtcbiAgICBpZiAoIWZpcnN0VGltZSAmJiB2YWx1ZSA9PT0gb2xkVmFsdWUpXG4gICAgICByZXR1cm47XG4gICAgaWYgKG1vZGlmaWVycy5pbmNsdWRlcyhcImltbWVkaWF0ZVwiKSlcbiAgICAgIHZhbHVlID8gY2xpY2tBd2F5Q29tcGF0aWJsZVNob3coKSA6IGhpZGUoKTtcbiAgICB0b2dnbGUodmFsdWUpO1xuICAgIG9sZFZhbHVlID0gdmFsdWU7XG4gICAgZmlyc3RUaW1lID0gZmFsc2U7XG4gIH0pKTtcbn0pO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy94LWZvci5qc1xuZGlyZWN0aXZlKFwiZm9yXCIsIChlbCwge2V4cHJlc3Npb259LCB7ZWZmZWN0OiBlZmZlY3QzLCBjbGVhbnVwOiBjbGVhbnVwMn0pID0+IHtcbiAgbGV0IGl0ZXJhdG9yTmFtZXMgPSBwYXJzZUZvckV4cHJlc3Npb24oZXhwcmVzc2lvbik7XG4gIGxldCBldmFsdWF0ZUl0ZW1zID0gZXZhbHVhdGVMYXRlcihlbCwgaXRlcmF0b3JOYW1lcy5pdGVtcyk7XG4gIGxldCBldmFsdWF0ZUtleSA9IGV2YWx1YXRlTGF0ZXIoZWwsIGVsLl94X2tleUV4cHJlc3Npb24gfHwgXCJpbmRleFwiKTtcbiAgZWwuX3hfcHJldktleXMgPSBbXTtcbiAgZWwuX3hfbG9va3VwID0ge307XG4gIGVmZmVjdDMoKCkgPT4gbG9vcChlbCwgaXRlcmF0b3JOYW1lcywgZXZhbHVhdGVJdGVtcywgZXZhbHVhdGVLZXkpKTtcbiAgY2xlYW51cDIoKCkgPT4ge1xuICAgIE9iamVjdC52YWx1ZXMoZWwuX3hfbG9va3VwKS5mb3JFYWNoKChlbDIpID0+IGVsMi5yZW1vdmUoKSk7XG4gICAgZGVsZXRlIGVsLl94X3ByZXZLZXlzO1xuICAgIGRlbGV0ZSBlbC5feF9sb29rdXA7XG4gIH0pO1xufSk7XG5mdW5jdGlvbiBsb29wKGVsLCBpdGVyYXRvck5hbWVzLCBldmFsdWF0ZUl0ZW1zLCBldmFsdWF0ZUtleSkge1xuICBsZXQgaXNPYmplY3QyID0gKGkpID0+IHR5cGVvZiBpID09PSBcIm9iamVjdFwiICYmICFBcnJheS5pc0FycmF5KGkpO1xuICBsZXQgdGVtcGxhdGVFbCA9IGVsO1xuICBldmFsdWF0ZUl0ZW1zKChpdGVtcykgPT4ge1xuICAgIGlmIChpc051bWVyaWMzKGl0ZW1zKSAmJiBpdGVtcyA+PSAwKSB7XG4gICAgICBpdGVtcyA9IEFycmF5LmZyb20oQXJyYXkoaXRlbXMpLmtleXMoKSwgKGkpID0+IGkgKyAxKTtcbiAgICB9XG4gICAgaWYgKGl0ZW1zID09PSB2b2lkIDApXG4gICAgICBpdGVtcyA9IFtdO1xuICAgIGxldCBsb29rdXAgPSBlbC5feF9sb29rdXA7XG4gICAgbGV0IHByZXZLZXlzID0gZWwuX3hfcHJldktleXM7XG4gICAgbGV0IHNjb3BlcyA9IFtdO1xuICAgIGxldCBrZXlzID0gW107XG4gICAgaWYgKGlzT2JqZWN0MihpdGVtcykpIHtcbiAgICAgIGl0ZW1zID0gT2JqZWN0LmVudHJpZXMoaXRlbXMpLm1hcCgoW2tleSwgdmFsdWVdKSA9PiB7XG4gICAgICAgIGxldCBzY29wZTIgPSBnZXRJdGVyYXRpb25TY29wZVZhcmlhYmxlcyhpdGVyYXRvck5hbWVzLCB2YWx1ZSwga2V5LCBpdGVtcyk7XG4gICAgICAgIGV2YWx1YXRlS2V5KCh2YWx1ZTIpID0+IGtleXMucHVzaCh2YWx1ZTIpLCB7c2NvcGU6IHtpbmRleDoga2V5LCAuLi5zY29wZTJ9fSk7XG4gICAgICAgIHNjb3Blcy5wdXNoKHNjb3BlMik7XG4gICAgICB9KTtcbiAgICB9IGVsc2Uge1xuICAgICAgZm9yIChsZXQgaSA9IDA7IGkgPCBpdGVtcy5sZW5ndGg7IGkrKykge1xuICAgICAgICBsZXQgc2NvcGUyID0gZ2V0SXRlcmF0aW9uU2NvcGVWYXJpYWJsZXMoaXRlcmF0b3JOYW1lcywgaXRlbXNbaV0sIGksIGl0ZW1zKTtcbiAgICAgICAgZXZhbHVhdGVLZXkoKHZhbHVlKSA9PiBrZXlzLnB1c2godmFsdWUpLCB7c2NvcGU6IHtpbmRleDogaSwgLi4uc2NvcGUyfX0pO1xuICAgICAgICBzY29wZXMucHVzaChzY29wZTIpO1xuICAgICAgfVxuICAgIH1cbiAgICBsZXQgYWRkcyA9IFtdO1xuICAgIGxldCBtb3ZlcyA9IFtdO1xuICAgIGxldCByZW1vdmVzID0gW107XG4gICAgbGV0IHNhbWVzID0gW107XG4gICAgZm9yIChsZXQgaSA9IDA7IGkgPCBwcmV2S2V5cy5sZW5ndGg7IGkrKykge1xuICAgICAgbGV0IGtleSA9IHByZXZLZXlzW2ldO1xuICAgICAgaWYgKGtleXMuaW5kZXhPZihrZXkpID09PSAtMSlcbiAgICAgICAgcmVtb3Zlcy5wdXNoKGtleSk7XG4gICAgfVxuICAgIHByZXZLZXlzID0gcHJldktleXMuZmlsdGVyKChrZXkpID0+ICFyZW1vdmVzLmluY2x1ZGVzKGtleSkpO1xuICAgIGxldCBsYXN0S2V5ID0gXCJ0ZW1wbGF0ZVwiO1xuICAgIGZvciAobGV0IGkgPSAwOyBpIDwga2V5cy5sZW5ndGg7IGkrKykge1xuICAgICAgbGV0IGtleSA9IGtleXNbaV07XG4gICAgICBsZXQgcHJldkluZGV4ID0gcHJldktleXMuaW5kZXhPZihrZXkpO1xuICAgICAgaWYgKHByZXZJbmRleCA9PT0gLTEpIHtcbiAgICAgICAgcHJldktleXMuc3BsaWNlKGksIDAsIGtleSk7XG4gICAgICAgIGFkZHMucHVzaChbbGFzdEtleSwgaV0pO1xuICAgICAgfSBlbHNlIGlmIChwcmV2SW5kZXggIT09IGkpIHtcbiAgICAgICAgbGV0IGtleUluU3BvdCA9IHByZXZLZXlzLnNwbGljZShpLCAxKVswXTtcbiAgICAgICAgbGV0IGtleUZvclNwb3QgPSBwcmV2S2V5cy5zcGxpY2UocHJldkluZGV4IC0gMSwgMSlbMF07XG4gICAgICAgIHByZXZLZXlzLnNwbGljZShpLCAwLCBrZXlGb3JTcG90KTtcbiAgICAgICAgcHJldktleXMuc3BsaWNlKHByZXZJbmRleCwgMCwga2V5SW5TcG90KTtcbiAgICAgICAgbW92ZXMucHVzaChba2V5SW5TcG90LCBrZXlGb3JTcG90XSk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICBzYW1lcy5wdXNoKGtleSk7XG4gICAgICB9XG4gICAgICBsYXN0S2V5ID0ga2V5O1xuICAgIH1cbiAgICBmb3IgKGxldCBpID0gMDsgaSA8IHJlbW92ZXMubGVuZ3RoOyBpKyspIHtcbiAgICAgIGxldCBrZXkgPSByZW1vdmVzW2ldO1xuICAgICAgaWYgKCEhbG9va3VwW2tleV0uX3hfZWZmZWN0cykge1xuICAgICAgICBsb29rdXBba2V5XS5feF9lZmZlY3RzLmZvckVhY2goZGVxdWV1ZUpvYik7XG4gICAgICB9XG4gICAgICBsb29rdXBba2V5XS5yZW1vdmUoKTtcbiAgICAgIGxvb2t1cFtrZXldID0gbnVsbDtcbiAgICAgIGRlbGV0ZSBsb29rdXBba2V5XTtcbiAgICB9XG4gICAgZm9yIChsZXQgaSA9IDA7IGkgPCBtb3Zlcy5sZW5ndGg7IGkrKykge1xuICAgICAgbGV0IFtrZXlJblNwb3QsIGtleUZvclNwb3RdID0gbW92ZXNbaV07XG4gICAgICBsZXQgZWxJblNwb3QgPSBsb29rdXBba2V5SW5TcG90XTtcbiAgICAgIGxldCBlbEZvclNwb3QgPSBsb29rdXBba2V5Rm9yU3BvdF07XG4gICAgICBsZXQgbWFya2VyID0gZG9jdW1lbnQuY3JlYXRlRWxlbWVudChcImRpdlwiKTtcbiAgICAgIG11dGF0ZURvbSgoKSA9PiB7XG4gICAgICAgIGVsRm9yU3BvdC5hZnRlcihtYXJrZXIpO1xuICAgICAgICBlbEluU3BvdC5hZnRlcihlbEZvclNwb3QpO1xuICAgICAgICBlbEZvclNwb3QuX3hfY3VycmVudElmRWwgJiYgZWxGb3JTcG90LmFmdGVyKGVsRm9yU3BvdC5feF9jdXJyZW50SWZFbCk7XG4gICAgICAgIG1hcmtlci5iZWZvcmUoZWxJblNwb3QpO1xuICAgICAgICBlbEluU3BvdC5feF9jdXJyZW50SWZFbCAmJiBlbEluU3BvdC5hZnRlcihlbEluU3BvdC5feF9jdXJyZW50SWZFbCk7XG4gICAgICAgIG1hcmtlci5yZW1vdmUoKTtcbiAgICAgIH0pO1xuICAgICAgcmVmcmVzaFNjb3BlKGVsRm9yU3BvdCwgc2NvcGVzW2tleXMuaW5kZXhPZihrZXlGb3JTcG90KV0pO1xuICAgIH1cbiAgICBmb3IgKGxldCBpID0gMDsgaSA8IGFkZHMubGVuZ3RoOyBpKyspIHtcbiAgICAgIGxldCBbbGFzdEtleTIsIGluZGV4XSA9IGFkZHNbaV07XG4gICAgICBsZXQgbGFzdEVsID0gbGFzdEtleTIgPT09IFwidGVtcGxhdGVcIiA/IHRlbXBsYXRlRWwgOiBsb29rdXBbbGFzdEtleTJdO1xuICAgICAgaWYgKGxhc3RFbC5feF9jdXJyZW50SWZFbClcbiAgICAgICAgbGFzdEVsID0gbGFzdEVsLl94X2N1cnJlbnRJZkVsO1xuICAgICAgbGV0IHNjb3BlMiA9IHNjb3Blc1tpbmRleF07XG4gICAgICBsZXQga2V5ID0ga2V5c1tpbmRleF07XG4gICAgICBsZXQgY2xvbmUyID0gZG9jdW1lbnQuaW1wb3J0Tm9kZSh0ZW1wbGF0ZUVsLmNvbnRlbnQsIHRydWUpLmZpcnN0RWxlbWVudENoaWxkO1xuICAgICAgYWRkU2NvcGVUb05vZGUoY2xvbmUyLCByZWFjdGl2ZShzY29wZTIpLCB0ZW1wbGF0ZUVsKTtcbiAgICAgIG11dGF0ZURvbSgoKSA9PiB7XG4gICAgICAgIGxhc3RFbC5hZnRlcihjbG9uZTIpO1xuICAgICAgICBpbml0VHJlZShjbG9uZTIpO1xuICAgICAgfSk7XG4gICAgICBpZiAodHlwZW9mIGtleSA9PT0gXCJvYmplY3RcIikge1xuICAgICAgICB3YXJuKFwieC1mb3Iga2V5IGNhbm5vdCBiZSBhbiBvYmplY3QsIGl0IG11c3QgYmUgYSBzdHJpbmcgb3IgYW4gaW50ZWdlclwiLCB0ZW1wbGF0ZUVsKTtcbiAgICAgIH1cbiAgICAgIGxvb2t1cFtrZXldID0gY2xvbmUyO1xuICAgIH1cbiAgICBmb3IgKGxldCBpID0gMDsgaSA8IHNhbWVzLmxlbmd0aDsgaSsrKSB7XG4gICAgICByZWZyZXNoU2NvcGUobG9va3VwW3NhbWVzW2ldXSwgc2NvcGVzW2tleXMuaW5kZXhPZihzYW1lc1tpXSldKTtcbiAgICB9XG4gICAgdGVtcGxhdGVFbC5feF9wcmV2S2V5cyA9IGtleXM7XG4gIH0pO1xufVxuZnVuY3Rpb24gcGFyc2VGb3JFeHByZXNzaW9uKGV4cHJlc3Npb24pIHtcbiAgbGV0IGZvckl0ZXJhdG9yUkUgPSAvLChbXixcXH1cXF1dKikoPzosKFteLFxcfVxcXV0qKSk/JC87XG4gIGxldCBzdHJpcFBhcmVuc1JFID0gL15cXHMqXFwofFxcKVxccyokL2c7XG4gIGxldCBmb3JBbGlhc1JFID0gLyhbXFxzXFxTXSo/KVxccysoPzppbnxvZilcXHMrKFtcXHNcXFNdKikvO1xuICBsZXQgaW5NYXRjaCA9IGV4cHJlc3Npb24ubWF0Y2goZm9yQWxpYXNSRSk7XG4gIGlmICghaW5NYXRjaClcbiAgICByZXR1cm47XG4gIGxldCByZXMgPSB7fTtcbiAgcmVzLml0ZW1zID0gaW5NYXRjaFsyXS50cmltKCk7XG4gIGxldCBpdGVtID0gaW5NYXRjaFsxXS5yZXBsYWNlKHN0cmlwUGFyZW5zUkUsIFwiXCIpLnRyaW0oKTtcbiAgbGV0IGl0ZXJhdG9yTWF0Y2ggPSBpdGVtLm1hdGNoKGZvckl0ZXJhdG9yUkUpO1xuICBpZiAoaXRlcmF0b3JNYXRjaCkge1xuICAgIHJlcy5pdGVtID0gaXRlbS5yZXBsYWNlKGZvckl0ZXJhdG9yUkUsIFwiXCIpLnRyaW0oKTtcbiAgICByZXMuaW5kZXggPSBpdGVyYXRvck1hdGNoWzFdLnRyaW0oKTtcbiAgICBpZiAoaXRlcmF0b3JNYXRjaFsyXSkge1xuICAgICAgcmVzLmNvbGxlY3Rpb24gPSBpdGVyYXRvck1hdGNoWzJdLnRyaW0oKTtcbiAgICB9XG4gIH0gZWxzZSB7XG4gICAgcmVzLml0ZW0gPSBpdGVtO1xuICB9XG4gIHJldHVybiByZXM7XG59XG5mdW5jdGlvbiBnZXRJdGVyYXRpb25TY29wZVZhcmlhYmxlcyhpdGVyYXRvck5hbWVzLCBpdGVtLCBpbmRleCwgaXRlbXMpIHtcbiAgbGV0IHNjb3BlVmFyaWFibGVzID0ge307XG4gIGlmICgvXlxcWy4qXFxdJC8udGVzdChpdGVyYXRvck5hbWVzLml0ZW0pICYmIEFycmF5LmlzQXJyYXkoaXRlbSkpIHtcbiAgICBsZXQgbmFtZXMgPSBpdGVyYXRvck5hbWVzLml0ZW0ucmVwbGFjZShcIltcIiwgXCJcIikucmVwbGFjZShcIl1cIiwgXCJcIikuc3BsaXQoXCIsXCIpLm1hcCgoaSkgPT4gaS50cmltKCkpO1xuICAgIG5hbWVzLmZvckVhY2goKG5hbWUsIGkpID0+IHtcbiAgICAgIHNjb3BlVmFyaWFibGVzW25hbWVdID0gaXRlbVtpXTtcbiAgICB9KTtcbiAgfSBlbHNlIGlmICgvXlxcey4qXFx9JC8udGVzdChpdGVyYXRvck5hbWVzLml0ZW0pICYmICFBcnJheS5pc0FycmF5KGl0ZW0pICYmIHR5cGVvZiBpdGVtID09PSBcIm9iamVjdFwiKSB7XG4gICAgbGV0IG5hbWVzID0gaXRlcmF0b3JOYW1lcy5pdGVtLnJlcGxhY2UoXCJ7XCIsIFwiXCIpLnJlcGxhY2UoXCJ9XCIsIFwiXCIpLnNwbGl0KFwiLFwiKS5tYXAoKGkpID0+IGkudHJpbSgpKTtcbiAgICBuYW1lcy5mb3JFYWNoKChuYW1lKSA9PiB7XG4gICAgICBzY29wZVZhcmlhYmxlc1tuYW1lXSA9IGl0ZW1bbmFtZV07XG4gICAgfSk7XG4gIH0gZWxzZSB7XG4gICAgc2NvcGVWYXJpYWJsZXNbaXRlcmF0b3JOYW1lcy5pdGVtXSA9IGl0ZW07XG4gIH1cbiAgaWYgKGl0ZXJhdG9yTmFtZXMuaW5kZXgpXG4gICAgc2NvcGVWYXJpYWJsZXNbaXRlcmF0b3JOYW1lcy5pbmRleF0gPSBpbmRleDtcbiAgaWYgKGl0ZXJhdG9yTmFtZXMuY29sbGVjdGlvbilcbiAgICBzY29wZVZhcmlhYmxlc1tpdGVyYXRvck5hbWVzLmNvbGxlY3Rpb25dID0gaXRlbXM7XG4gIHJldHVybiBzY29wZVZhcmlhYmxlcztcbn1cbmZ1bmN0aW9uIGlzTnVtZXJpYzMoc3ViamVjdCkge1xuICByZXR1cm4gIUFycmF5LmlzQXJyYXkoc3ViamVjdCkgJiYgIWlzTmFOKHN1YmplY3QpO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy94LXJlZi5qc1xuZnVuY3Rpb24gaGFuZGxlcjIoKSB7XG59XG5oYW5kbGVyMi5pbmxpbmUgPSAoZWwsIHtleHByZXNzaW9ufSwge2NsZWFudXA6IGNsZWFudXAyfSkgPT4ge1xuICBsZXQgcm9vdCA9IGNsb3Nlc3RSb290KGVsKTtcbiAgaWYgKCFyb290Ll94X3JlZnMpXG4gICAgcm9vdC5feF9yZWZzID0ge307XG4gIHJvb3QuX3hfcmVmc1tleHByZXNzaW9uXSA9IGVsO1xuICBjbGVhbnVwMigoKSA9PiBkZWxldGUgcm9vdC5feF9yZWZzW2V4cHJlc3Npb25dKTtcbn07XG5kaXJlY3RpdmUoXCJyZWZcIiwgaGFuZGxlcjIpO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy94LWlmLmpzXG5kaXJlY3RpdmUoXCJpZlwiLCAoZWwsIHtleHByZXNzaW9ufSwge2VmZmVjdDogZWZmZWN0MywgY2xlYW51cDogY2xlYW51cDJ9KSA9PiB7XG4gIGxldCBldmFsdWF0ZTIgPSBldmFsdWF0ZUxhdGVyKGVsLCBleHByZXNzaW9uKTtcbiAgbGV0IHNob3cgPSAoKSA9PiB7XG4gICAgaWYgKGVsLl94X2N1cnJlbnRJZkVsKVxuICAgICAgcmV0dXJuIGVsLl94X2N1cnJlbnRJZkVsO1xuICAgIGxldCBjbG9uZTIgPSBlbC5jb250ZW50LmNsb25lTm9kZSh0cnVlKS5maXJzdEVsZW1lbnRDaGlsZDtcbiAgICBhZGRTY29wZVRvTm9kZShjbG9uZTIsIHt9LCBlbCk7XG4gICAgbXV0YXRlRG9tKCgpID0+IHtcbiAgICAgIGVsLmFmdGVyKGNsb25lMik7XG4gICAgICBpbml0VHJlZShjbG9uZTIpO1xuICAgIH0pO1xuICAgIGVsLl94X2N1cnJlbnRJZkVsID0gY2xvbmUyO1xuICAgIGVsLl94X3VuZG9JZiA9ICgpID0+IHtcbiAgICAgIHdhbGsoY2xvbmUyLCAobm9kZSkgPT4ge1xuICAgICAgICBpZiAoISFub2RlLl94X2VmZmVjdHMpIHtcbiAgICAgICAgICBub2RlLl94X2VmZmVjdHMuZm9yRWFjaChkZXF1ZXVlSm9iKTtcbiAgICAgICAgfVxuICAgICAgfSk7XG4gICAgICBjbG9uZTIucmVtb3ZlKCk7XG4gICAgICBkZWxldGUgZWwuX3hfY3VycmVudElmRWw7XG4gICAgfTtcbiAgICByZXR1cm4gY2xvbmUyO1xuICB9O1xuICBsZXQgaGlkZSA9ICgpID0+IHtcbiAgICBpZiAoIWVsLl94X3VuZG9JZilcbiAgICAgIHJldHVybjtcbiAgICBlbC5feF91bmRvSWYoKTtcbiAgICBkZWxldGUgZWwuX3hfdW5kb0lmO1xuICB9O1xuICBlZmZlY3QzKCgpID0+IGV2YWx1YXRlMigodmFsdWUpID0+IHtcbiAgICB2YWx1ZSA/IHNob3coKSA6IGhpZGUoKTtcbiAgfSkpO1xuICBjbGVhbnVwMigoKSA9PiBlbC5feF91bmRvSWYgJiYgZWwuX3hfdW5kb0lmKCkpO1xufSk7XG5cbi8vIHBhY2thZ2VzL2FscGluZWpzL3NyYy9kaXJlY3RpdmVzL3gtaWQuanNcbmRpcmVjdGl2ZShcImlkXCIsIChlbCwge2V4cHJlc3Npb259LCB7ZXZhbHVhdGU6IGV2YWx1YXRlMn0pID0+IHtcbiAgbGV0IG5hbWVzID0gZXZhbHVhdGUyKGV4cHJlc3Npb24pO1xuICBuYW1lcy5mb3JFYWNoKChuYW1lKSA9PiBzZXRJZFJvb3QoZWwsIG5hbWUpKTtcbn0pO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy94LW9uLmpzXG5tYXBBdHRyaWJ1dGVzKHN0YXJ0aW5nV2l0aChcIkBcIiwgaW50byhwcmVmaXgoXCJvbjpcIikpKSk7XG5kaXJlY3RpdmUoXCJvblwiLCBza2lwRHVyaW5nQ2xvbmUoKGVsLCB7dmFsdWUsIG1vZGlmaWVycywgZXhwcmVzc2lvbn0sIHtjbGVhbnVwOiBjbGVhbnVwMn0pID0+IHtcbiAgbGV0IGV2YWx1YXRlMiA9IGV4cHJlc3Npb24gPyBldmFsdWF0ZUxhdGVyKGVsLCBleHByZXNzaW9uKSA6ICgpID0+IHtcbiAgfTtcbiAgaWYgKGVsLnRhZ05hbWUudG9Mb3dlckNhc2UoKSA9PT0gXCJ0ZW1wbGF0ZVwiKSB7XG4gICAgaWYgKCFlbC5feF9mb3J3YXJkRXZlbnRzKVxuICAgICAgZWwuX3hfZm9yd2FyZEV2ZW50cyA9IFtdO1xuICAgIGlmICghZWwuX3hfZm9yd2FyZEV2ZW50cy5pbmNsdWRlcyh2YWx1ZSkpXG4gICAgICBlbC5feF9mb3J3YXJkRXZlbnRzLnB1c2godmFsdWUpO1xuICB9XG4gIGxldCByZW1vdmVMaXN0ZW5lciA9IG9uKGVsLCB2YWx1ZSwgbW9kaWZpZXJzLCAoZSkgPT4ge1xuICAgIGV2YWx1YXRlMigoKSA9PiB7XG4gICAgfSwge3Njb3BlOiB7JGV2ZW50OiBlfSwgcGFyYW1zOiBbZV19KTtcbiAgfSk7XG4gIGNsZWFudXAyKCgpID0+IHJlbW92ZUxpc3RlbmVyKCkpO1xufSkpO1xuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvZGlyZWN0aXZlcy9pbmRleC5qc1xud2Fybk1pc3NpbmdQbHVnaW5EaXJlY3RpdmUoXCJDb2xsYXBzZVwiLCBcImNvbGxhcHNlXCIsIFwiY29sbGFwc2VcIik7XG53YXJuTWlzc2luZ1BsdWdpbkRpcmVjdGl2ZShcIkludGVyc2VjdFwiLCBcImludGVyc2VjdFwiLCBcImludGVyc2VjdFwiKTtcbndhcm5NaXNzaW5nUGx1Z2luRGlyZWN0aXZlKFwiRm9jdXNcIiwgXCJ0cmFwXCIsIFwiZm9jdXNcIik7XG53YXJuTWlzc2luZ1BsdWdpbkRpcmVjdGl2ZShcIk1hc2tcIiwgXCJtYXNrXCIsIFwibWFza1wiKTtcbmZ1bmN0aW9uIHdhcm5NaXNzaW5nUGx1Z2luRGlyZWN0aXZlKG5hbWUsIGRpcmVjdGl2ZU5hbWUyLCBzbHVnKSB7XG4gIGRpcmVjdGl2ZShkaXJlY3RpdmVOYW1lMiwgKGVsKSA9PiB3YXJuKGBZb3UgY2FuJ3QgdXNlIFt4LSR7ZGlyZWN0aXZlTmFtZTJ9XSB3aXRob3V0IGZpcnN0IGluc3RhbGxpbmcgdGhlIFwiJHtuYW1lfVwiIHBsdWdpbiBoZXJlOiBodHRwczovL2FscGluZWpzLmRldi9wbHVnaW5zLyR7c2x1Z31gLCBlbCkpO1xufVxuXG4vLyBwYWNrYWdlcy9hbHBpbmVqcy9zcmMvaW5kZXguanNcbmFscGluZV9kZWZhdWx0LnNldEV2YWx1YXRvcihub3JtYWxFdmFsdWF0b3IpO1xuYWxwaW5lX2RlZmF1bHQuc2V0UmVhY3Rpdml0eUVuZ2luZSh7cmVhY3RpdmU6IHJlYWN0aXZlMiwgZWZmZWN0OiBlZmZlY3QyLCByZWxlYXNlOiBzdG9wLCByYXc6IHRvUmF3fSk7XG52YXIgc3JjX2RlZmF1bHQgPSBhbHBpbmVfZGVmYXVsdDtcblxuLy8gcGFja2FnZXMvYWxwaW5lanMvYnVpbGRzL21vZHVsZS5qc1xudmFyIG1vZHVsZV9kZWZhdWx0ID0gc3JjX2RlZmF1bHQ7XG5leHBvcnQge1xuICBtb2R1bGVfZGVmYXVsdCBhcyBkZWZhdWx0XG59O1xuIiwiZXhwb3J0IGRlZmF1bHQgKCkgPT4ge1xuICAgIGRvY3VtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJzdWJtaXRcIiwgKGV2ZW50OiBTdWJtaXRFdmVudCkgPT4ge1xuICAgICAgICBjb25zdCB0YXJnZXQgPSBldmVudC50YXJnZXQgYXMgSFRNTEZvcm1FbGVtZW50O1xuICAgICAgICBpZiAoIXRhcmdldCB8fCAhdGFyZ2V0LmNsYXNzTGlzdC5jb250YWlucyhcImpzLWFqYXgtZm9ybS1zdWJtaXRcIikpIHsgcmV0dXJuOyB9XG5cbiAgICAgICAgZXZlbnQucHJldmVudERlZmF1bHQoKTtcblxuICAgICAgICB2YXIgZGF0YSA9IG5ldyBGb3JtRGF0YSh0YXJnZXQpO1xuICAgICAgICBkYXRhLmFwcGVuZChcImlzQWpheFwiLCBcInRydWVcIik7XG4gICAgICAgIGlmICgoZXZlbnQuc3VibWl0dGVyIGFzIEhUTUxCdXR0b25FbGVtZW50KT8ubmFtZSl7XG4gICAgICAgICAgICBjb25zdCBidXR0b25TdWJtaXR0ZXIgPSBldmVudC5zdWJtaXR0ZXIgYXMgSFRNTEJ1dHRvbkVsZW1lbnQ7XG4gICAgICAgICAgICBkYXRhLmFwcGVuZChidXR0b25TdWJtaXR0ZXIubmFtZSwgYnV0dG9uU3VibWl0dGVyLnZhbHVlKTtcbiAgICAgICAgfVxuXG4gICAgICAgIGZldGNoKHRhcmdldC5hY3Rpb24sIHtcbiAgICAgICAgICAgIG1ldGhvZDogJ1BPU1QnLFxuICAgICAgICAgICAgYm9keTogZGF0YVxuICAgICAgICB9KS50aGVuKHJlcyA9PiByZXMuanNvbigpKVxuICAgICAgICAgICAgLnRoZW4oKHJlczogQWpheFJlc3BvbnNlIHwgQWpheFJlc3BvbnNlW10pID0+IHtcbiAgICAgICAgICAgICAgICB0YXJnZXQucmVzZXQoKTtcblxuICAgICAgICAgICAgICAgIGlmIChBcnJheS5pc0FycmF5KHJlcykpIHtcbiAgICAgICAgICAgICAgICAgICAgcmVzLmZvckVhY2goaXRlbSA9PiB7XG4gICAgICAgICAgICAgICAgICAgICAgICB1cGRhdGVEb21Db250ZW50KGl0ZW0pO1xuICAgICAgICAgICAgICAgICAgICB9KTtcbiAgICAgICAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgICAgICAgICB1cGRhdGVEb21Db250ZW50KHJlcyk7XG4gICAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgfSk7XG5cbiAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgIH0pO1xufVxuXG5mdW5jdGlvbiB1cGRhdGVEb21Db250ZW50KHJlczogQWpheFJlc3BvbnNlKSB7XG4gICAgY29uc3QgZWxlbWVudCA9IGRvY3VtZW50LmdldEVsZW1lbnRCeUlkKHJlcy5pZCk7XG4gICAgaWYgKHJlcy5pc0lubmVyKSB7XG4gICAgICAgIGVsZW1lbnQuaW5uZXJIVE1MID0gcmVzLmNvbnRlbnQ7XG4gICAgfVxuICAgIGVsc2Uge1xuICAgICAgICBlbGVtZW50Lm91dGVySFRNTCA9IHJlcy5jb250ZW50O1xuICAgIH1cbiAgICBcbiAgICAvLyBAdHMtaWdub3JlXG4gICAgaHRteC5wcm9jZXNzKGVsZW1lbnQpO1xufVxuXG5pbnRlcmZhY2UgQWpheFJlc3BvbnNlIHtcbiAgICBpZDogc3RyaW5nO1xuICAgIGNvbnRlbnQ6IHN0cmluZztcbiAgICBpc0lubmVyOiBib29sZWFuO1xufSIsImltcG9ydCB7IGFycm93LCBjb21wdXRlUG9zaXRpb24sIGZsaXAsIG9mZnNldCwgc2hpZnQgfSBmcm9tIFwiQGZsb2F0aW5nLXVpL2RvbVwiO1xuXG5leHBvcnQgZGVmYXVsdCAoKSA9PiB7XG4gIGRvY3VtZW50XG4gICAgLnF1ZXJ5U2VsZWN0b3JBbGwoXCIuYW5jaG9yLXNvdXJjZVwiKVxuICAgIC5mb3JFYWNoKChlbGVtZW50OiBIVE1MRWxlbWVudCkgPT4ge1xuICAgICAgY29uc3QgdGFyZ2V0SWQgPSBlbGVtZW50LmdldEF0dHJpYnV0ZShcImRhdGEtYW5jaG9yLXRhcmdldC1pZFwiKTtcbiAgICAgIGNvbnN0IHRhcmdldCA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjXCIgKyB0YXJnZXRJZCkgYXMgSFRNTEVsZW1lbnQ7XG5cbiAgICAgIGlmICghdGFyZ2V0KSB7XG4gICAgICAgIHJldHVybjtcbiAgICAgIH1cblxuICAgICAgZWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgb3Blbik7XG5cbiAgICAgIGZ1bmN0aW9uIG9wZW4oZXZlbnQ6IE1vdXNlRXZlbnQpIHtcbiAgICAgICAgZXZlbnQuc3RvcFByb3BhZ2F0aW9uKCk7XG4gICAgXG4gICAgICAgIHRhcmdldC5jbGFzc0xpc3QucmVtb3ZlKFwiaGlkZGVuXCIpO1xuICAgICAgICB1cGRhdGUoZWxlbWVudCwgdGFyZ2V0KTtcbiAgICBcbiAgICAgICAgZWxlbWVudC5yZW1vdmVFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgb3Blbik7XG4gICAgICAgIGVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIGNsb3NlKTtcbiAgICAgICAgd2luZG93LmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCB0cnlDbG9zZSk7XG4gICAgICB9XG4gICAgXG4gICAgICBmdW5jdGlvbiB0cnlDbG9zZShldmVudDogTW91c2VFdmVudCkge1xuICAgICAgICBjb25zdCBjbGlja1RhcmdldCA9IGV2ZW50LnRhcmdldCBhcyBIVE1MRWxlbWVudDtcbiAgICAgICAgaWYgKCFjbGlja1RhcmdldC5jbG9zZXN0KHRhcmdldC5pZCkpe1xuICAgICAgICAgICAgY2xvc2UoKTtcbiAgICAgICAgfVxuICAgICAgfVxuICAgIFxuICAgICAgZnVuY3Rpb24gY2xvc2UoKSB7XG4gICAgICAgIHRhcmdldC5jbGFzc0xpc3QuYWRkKFwiaGlkZGVuXCIpO1xuICAgIFxuICAgICAgICBlbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJjbGlja1wiLCBvcGVuKTtcbiAgICAgICAgZWxlbWVudC5yZW1vdmVFdmVudExpc3RlbmVyKFwiY2xpY2tcIiwgY2xvc2UpO1xuICAgICAgICB3aW5kb3cucmVtb3ZlRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIHRyeUNsb3NlKTtcbiAgICAgIH1cbiAgICB9KTtcblxuICBmdW5jdGlvbiB1cGRhdGUoYnV0dG9uRWxlbTogSFRNTEVsZW1lbnQsIHRhcmdldEVsZW06IEhUTUxFbGVtZW50KSB7XG4gICAgY29uc3QgYXJyb3dJZCA9IGJ1dHRvbkVsZW0uZ2V0QXR0cmlidXRlKFwiZGF0YS1hbmNob3ItYXJyb3ctaWRcIik7XG4gICAgY29uc3QgYXJyb3dFbCA9IGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3IoXCIjXCIgKyBhcnJvd0lkKSBhcyBIVE1MRWxlbWVudDtcblxuICAgIGNvbXB1dGVQb3NpdGlvbihidXR0b25FbGVtLCB0YXJnZXRFbGVtLCB7XG4gICAgICBwbGFjZW1lbnQ6IFwidG9wLXN0YXJ0XCIsXG4gICAgICBtaWRkbGV3YXJlOiBbb2Zmc2V0KDYpLCBmbGlwKCksIHNoaWZ0KCksIGFycm93KHsgZWxlbWVudDogYXJyb3dFbCB9KV0sXG4gICAgfSkudGhlbigoeyB4LCB5LCBwbGFjZW1lbnQsIG1pZGRsZXdhcmVEYXRhIH0pID0+IHtcbiAgICAgIE9iamVjdC5hc3NpZ24odGFyZ2V0RWxlbS5zdHlsZSwge1xuICAgICAgICBsZWZ0OiBgJHt4fXB4YCxcbiAgICAgICAgdG9wOiBgJHt5fXB4YCxcbiAgICAgIH0pO1xuXG4gICAgICAvLyBBY2Nlc3NpbmcgdGhlIGRhdGFcbiAgICAgIGNvbnN0IHsgeDogYXJyb3dYLCB5OiBhcnJvd1kgfSA9IG1pZGRsZXdhcmVEYXRhLmFycm93O1xuXG4gICAgICBjb25zdCBzdGF0aWNTaWRlID0ge1xuICAgICAgICB0b3A6IFwiYm90dG9tXCIsXG4gICAgICAgIHJpZ2h0OiBcImxlZnRcIixcbiAgICAgICAgYm90dG9tOiBcInRvcFwiLFxuICAgICAgICBsZWZ0OiBcInJpZ2h0XCIsXG4gICAgICB9W3BsYWNlbWVudC5zcGxpdChcIi1cIilbMF1dO1xuXG4gICAgICBPYmplY3QuYXNzaWduKGFycm93RWwuc3R5bGUsIHtcbiAgICAgICAgbGVmdDogYXJyb3dYICE9IG51bGwgPyBgJHthcnJvd1h9cHhgIDogXCJcIixcbiAgICAgICAgdG9wOiBhcnJvd1kgIT0gbnVsbCA/IGAke2Fycm93WX1weGAgOiBcIlwiLFxuICAgICAgICByaWdodDogXCJcIixcbiAgICAgICAgYm90dG9tOiBcIlwiLFxuICAgICAgICBbc3RhdGljU2lkZV06IFwiLTRweFwiLFxuICAgICAgfSk7XG4gICAgfSk7XG4gIH1cbn07XG4iLCJleHBvcnQgZGVmYXVsdCAoKSA9PiB7XG4gICAgZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbChcIi5qcy1jbGFzcy1ob3ZlclwiKS5mb3JFYWNoKGVsZW1lbnQgPT4ge1xuICAgICAgICBlbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoXCJtb3VzZWVudGVyXCIsIChldmVudCkgPT4ge1xuICAgICAgICAgICAgY29uc3QgdGFyZ2V0RWxlbWVudCA9IGV2ZW50LmN1cnJlbnRUYXJnZXQgYXMgSFRNTEFuY2hvckVsZW1lbnQ7XG4gICAgICAgICAgICBjb25zdCBjbGFzc1RvQWRkID0gdGFyZ2V0RWxlbWVudC5nZXRBdHRyaWJ1dGUoXCJqcy1jbGFzcy1ob3ZlclwiKTtcblxuICAgICAgICAgICAgdGFyZ2V0RWxlbWVudC5jbGFzc0xpc3QuYWRkKGNsYXNzVG9BZGQpO1xuICAgICAgICB9KTtcbiAgICAgICAgZWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwibW91c2VsZWF2ZVwiLCAoZXZlbnQpID0+IHtcbiAgICAgICAgICAgIGNvbnN0IHRhcmdldEVsZW1lbnQgPSBldmVudC5jdXJyZW50VGFyZ2V0IGFzIEhUTUxBbmNob3JFbGVtZW50O1xuICAgICAgICAgICAgY29uc3QgY2xhc3NUb0FkZCA9IHRhcmdldEVsZW1lbnQuZ2V0QXR0cmlidXRlKFwianMtY2xhc3MtaG92ZXJcIik7XG5cbiAgICAgICAgICAgIHRhcmdldEVsZW1lbnQuY2xhc3NMaXN0LnJlbW92ZShjbGFzc1RvQWRkKTtcbiAgICAgICAgfSlcbiAgICB9KVxufSIsImV4cG9ydCBkZWZhdWx0ICgpID0+ICh7XG4gICAgc3RlcDogMCxcbiAgICBsb2FkaW5nOiBmYWxzZSxcbiAgICBmaWxlOiB1bmRlZmluZWQsXG4gICAgZXJyb3JNZXNzYWdlOiB1bmRlZmluZWQsXG5cbiAgICBzdWJtaXRGb3JtKGV2ZW50OiBTdWJtaXRFdmVudCkge1xuICAgICAgICBjb25zdCB0YXJnZXQgPSBldmVudC50YXJnZXQgYXMgSFRNTEZvcm1FbGVtZW50O1xuXG4gICAgICAgIGV2ZW50LnByZXZlbnREZWZhdWx0KCk7XG5cbiAgICAgICAgY29uc3QgZGF0YSA9IG5ldyBGb3JtRGF0YSh0YXJnZXQpO1xuXG4gICAgICAgIHRoaXMubG9hZGluZyA9IHRydWU7XG4gICAgICAgIHRoaXMuZXJyb3JNZXNzYWdlID0gdW5kZWZpbmVkO1xuICAgICAgICBmZXRjaCh0YXJnZXQuYWN0aW9uLCB7XG4gICAgICAgICAgICBtZXRob2Q6ICdQT1NUJyxcbiAgICAgICAgICAgIGJvZHk6IGRhdGFcbiAgICAgICAgfSkudGhlbigocmVzcCkgPT4ge1xuICAgICAgICAgICAgaWYgKHJlc3Aub2spIHtcbiAgICAgICAgICAgICAgICB0YXJnZXQucmVzZXQoKTtcblxuICAgICAgICAgICAgICAgIHRoaXMubG9hZGluZyA9IGZhbHNlO1xuICAgICAgICAgICAgICAgIHRoaXMuc3RlcCA9IDE7XG4gICAgICAgICAgICB9IGVsc2Uge1xuICAgICAgICAgICAgICAgIHRoaXMubG9hZGluZyA9IGZhbHNlO1xuICAgICAgICAgICAgICAgIHJlc3AudGV4dCgpLnRoZW4oKHRleHQpID0+IHtcbiAgICAgICAgICAgICAgICAgICAgdGhpcy5lcnJvck1lc3NhZ2UgPSB0ZXh0O1xuICAgICAgICAgICAgICAgIH0pO1xuICAgICAgICAgICAgfVxuICAgICAgICB9LCAoZXJyb3IpID0+IHtcbiAgICAgICAgICAgIGNvbnNvbGUubG9nKGVycm9yKTtcbiAgICAgICAgICAgIHRoaXMubG9hZGluZyA9IGZhbHNlO1xuICAgICAgICB9KTtcblxuICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgfSxcbn0pIiwiZXhwb3J0IGRlZmF1bHQgKCkgPT4ge1xuICBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiW2pzLWxpc3RdXCIpLmZvckVhY2goKGVsZW06IEhUTUxFbGVtZW50KSA9PiB7XG4gICAgbGV0IGN1cnJlbnRWYWx1ZSA9XG4gICAgICBlbGVtLmdldEF0dHJpYnV0ZShcImpzLWxpc3QtdmFsdWVcIikgPT0gXCJUcnVlXCIgPyB0cnVlIDogZmFsc2U7XG4gICAgY29uc3QgbGlzdCA9IGVsZW0uZ2V0QXR0cmlidXRlKFwianMtbGlzdFwiKTtcbiAgICBjb25zdCBzZXRJZCA9IGVsZW0uZ2V0QXR0cmlidXRlKFwianMtc2V0XCIpO1xuICAgIGVsZW0uYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsICgpID0+IHtcbiAgICAgIGZldGNoKFwiL3VtYnJhY28vYXBpL2NvbGxlY3Rpb24vdXBkYXRlU2V0SW5MaXN0XCIsIHtcbiAgICAgICAgYm9keTogSlNPTi5zdHJpbmdpZnkoe1xuICAgICAgICAgIHNldElkOiBzZXRJZCxcbiAgICAgICAgICBsaXN0TmFtZTogbGlzdCxcbiAgICAgICAgICB2YWx1ZTogY3VycmVudFZhbHVlLFxuICAgICAgICB9KSxcbiAgICAgICAgbWV0aG9kOiBcIlBPU1RcIixcbiAgICAgICAgaGVhZGVyczoge1xuICAgICAgICAgIFwiQ29udGVudC1UeXBlXCI6IFwiYXBwbGljYXRpb24vanNvblwiLFxuICAgICAgICB9LFxuICAgICAgfSlcbiAgICAgIC50aGVuKChyZXNwKSA9PiByZXNwLmpzb24oKSlcbiAgICAgIC50aGVuKChyZXNwKSA9PiB7XG4gICAgICAgIGVsZW0uaW5uZXJUZXh0ID0gcmVzcC50ZXh0O1xuICAgICAgICBjdXJyZW50VmFsdWUgPSAhY3VycmVudFZhbHVlO1xuICAgICAgfSk7XG4gICAgfSk7XG4gIH0pO1xufTtcbiIsImV4cG9ydCBkZWZhdWx0ICgpID0+IHtcblxuICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIuanMtY29weS1jbGlwYm9hcmQtYnV0dG9uXCIpLmZvckVhY2goZWxlbWVudCA9PiB7XG4gICAgICAgIGVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIChldmVudCkgPT4ge1xuICAgICAgICAgICAgY29uc3QgdGFyZ2V0RWxlbWVudCA9IGV2ZW50LmN1cnJlbnRUYXJnZXQgYXMgSFRNTEFuY2hvckVsZW1lbnQ7XG4gICAgICAgICAgICBjb25zdCB1cmwgPSB0YXJnZXRFbGVtZW50LmhyZWY7XG5cbiAgICAgICAgICAgIGZldGNoKHVybClcbiAgICAgICAgICAgICAgICAudGhlbigocmVzcG9uc2UpID0+IHJlc3BvbnNlLnRleHQoKSlcbiAgICAgICAgICAgICAgICAudGhlbigocmVzcG9uc2UpID0+IHtcbiAgICAgICAgICAgICAgICAgICAgbmF2aWdhdG9yLmNsaXBib2FyZC53cml0ZVRleHQocmVzcG9uc2UpO1xuICAgICAgICAgICAgICAgICAgICB0YXJnZXRFbGVtZW50LmdldEVsZW1lbnRzQnlUYWdOYW1lKFwicFwiKVswXS50ZXh0Q29udGVudCA9IFwiQ29waWVkIHRvIGNsaXBib2FyZFwiO1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICk7XG5cbiAgICAgICAgICAgIGV2ZW50LnByZXZlbnREZWZhdWx0KCk7XG4gICAgICAgIH0pO1xuICAgIH0pO1xufSIsImV4cG9ydCBkZWZhdWx0ICgpID0+IHtcbiAgICBjb25zdCBjdXJzb3JJbWFnZUVsZW0gPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKFwiI2N1cnNvci1pbWFnZVwiKSBhcyBIVE1MRWxlbWVudDtcbiAgICBpZiAoIWN1cnNvckltYWdlRWxlbSkgeyByZXR1cm47IH1cblxuICAgIGNvbnN0IGN1cnNvckltYWdlRWxlbVJlY3QgPSBjdXJzb3JJbWFnZUVsZW0uZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCk7XG5cbiAgICBsZXQgY3VycmVudFNvdXJjZUN1cnNvckltYWdlOiBIVE1MRWxlbWVudCA9IG51bGw7XG4gICAgd2luZG93LmFkZEV2ZW50TGlzdGVuZXIoXCJtb3VzZW92ZXJcIiwgKGV2ZW50OiBNb3VzZUV2ZW50KSA9PiB7XG4gICAgICAgIGNvbnN0IHNvdXJjZUN1cnNvckltYWdlID0gKGV2ZW50LnRhcmdldCBhcyBIVE1MRWxlbWVudCkuY2xvc2VzdCgnLmN1cnNvci1zb3VyY2UnKSBhcyBIVE1MRWxlbWVudDtcbiAgICAgICAgaWYgKCFzb3VyY2VDdXJzb3JJbWFnZSkge1xuICAgICAgICAgICAgZGVzZWxlY3RDdXJyZW50U291cmNlKCk7XG4gICAgICAgICAgICByZXR1cm47XG4gICAgICAgIH1cblxuICAgICAgICBjdXJyZW50U291cmNlQ3Vyc29ySW1hZ2UgPSBzb3VyY2VDdXJzb3JJbWFnZTtcblxuICAgICAgICBjdXJzb3JJbWFnZUVsZW0uc3R5bGUuYmFja2dyb3VuZEltYWdlID0gYHVybCgke3NvdXJjZUN1cnNvckltYWdlLmdldEF0dHJpYnV0ZShcImRhdGEtY3Vyc29yLWltYWdlXCIpfSlgO1xuXG4gICAgICAgIHdpbmRvdy5hZGRFdmVudExpc3RlbmVyKFwibW91c2Vtb3ZlXCIsIHVwZGF0ZUN1cnNvckltYWdlTG9jYXRpb24pO1xuICAgIH0pO1xuXG4gICAgZnVuY3Rpb24gZGVzZWxlY3RDdXJyZW50U291cmNlKCkge1xuICAgICAgICBpZiAoY3VycmVudFNvdXJjZUN1cnNvckltYWdlKSB7XG4gICAgICAgICAgICB3aW5kb3cucmVtb3ZlRXZlbnRMaXN0ZW5lcihcIm1vdXNlbW92ZVwiLCB1cGRhdGVDdXJzb3JJbWFnZUxvY2F0aW9uKTtcbiAgICAgICAgICAgIGN1cnNvckltYWdlRWxlbS5zdHlsZS5iYWNrZ3JvdW5kSW1hZ2UgPSBudWxsO1xuICAgICAgICAgICAgY3VycmVudFNvdXJjZUN1cnNvckltYWdlID0gbnVsbDtcbiAgICAgICAgfVxuICAgIH1cblxuICAgIGZ1bmN0aW9uIHVwZGF0ZUN1cnNvckltYWdlTG9jYXRpb24oZXZlbnQ6IE1vdXNlRXZlbnQpIHtcbiAgICAgICAgaWYgKGN1cnNvckltYWdlRWxlbVJlY3QuaGVpZ2h0ICsgZXZlbnQuY2xpZW50WSA+PSB3aW5kb3cuaW5uZXJIZWlnaHQpIHtcbiAgICAgICAgICAgIGN1cnNvckltYWdlRWxlbS5zdHlsZS50b3AgPSBgJHtldmVudC5wYWdlWSAtIGN1cnNvckltYWdlRWxlbVJlY3QuaGVpZ2h0fXB4YDtcbiAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIGN1cnNvckltYWdlRWxlbS5zdHlsZS50b3AgPSBgJHtldmVudC5wYWdlWSAtIDIwfXB4YDtcbiAgICAgICAgfVxuXG4gICAgICAgIGlmIChjdXJzb3JJbWFnZUVsZW1SZWN0LndpZHRoICsgZXZlbnQuY2xpZW50WCArIDQwID49IHdpbmRvdy5pbm5lcldpZHRoKSB7XG4gICAgICAgICAgICBjdXJzb3JJbWFnZUVsZW0uc3R5bGUubGVmdCA9IGAke2V2ZW50LnBhZ2VYIC0gY3Vyc29ySW1hZ2VFbGVtUmVjdC53aWR0aCAtIDIwfXB4YDtcbiAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIGN1cnNvckltYWdlRWxlbS5zdHlsZS5sZWZ0ID0gYCR7ZXZlbnQucGFnZVggKyAyMH1weGA7XG4gICAgICAgIH1cbiAgICB9XG59IiwiaW50ZXJmYWNlIERlY2tQcm9ncmVzc1Jlc3VsdCB7XG4gIGRlY2tJZDogbnVtYmVyO1xuICBwcm9ncmVzczogbnVtYmVyO1xufVxuXG5mdW5jdGlvbiBsb2FkRGVja0NvbGxlY3Rpb25Qcm9ncmVzcygpIHtcbiAgY29uc3QgZWxlbWVudHMgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsPEhUTUxFbGVtZW50PignW2RhdGEtZGVjay1jb2xsZWN0aW9uXScpO1xuICBpZiAoZWxlbWVudHMubGVuZ3RoID09PSAwKSByZXR1cm47XG5cbiAgY29uc3QgZGVja0lkcyA9IEFycmF5LmZyb20oZWxlbWVudHMpXG4gICAgLm1hcCgoZWwpID0+IGVsLmdldEF0dHJpYnV0ZSgnZGF0YS1kZWNrLWNvbGxlY3Rpb24nKSlcbiAgICAuZmlsdGVyKChpZCkgPT4gaWQgIT09IG51bGwpO1xuXG4gIGNvbnN0IHF1ZXJ5U3RyaW5nID0gZGVja0lkcy5tYXAoKGlkKSA9PiBgZGVja0lkcz0ke2lkfWApLmpvaW4oJyYnKTtcbiAgZmV0Y2goYC91bWJyYWNvL2FwaS9jb2xsZWN0aW9uL2dldGRlY2tzcHJvZ3Jlc3M/JHtxdWVyeVN0cmluZ31gKVxuICAgIC50aGVuKChyZXMpID0+IHJlcy5qc29uKCkpXG4gICAgLnRoZW4oKGRhdGE6IERlY2tQcm9ncmVzc1Jlc3VsdFtdKSA9PiB7XG4gICAgICBkYXRhLmZvckVhY2goKGl0ZW0pID0+IHtcbiAgICAgICAgY29uc3QgZWwgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKGBbZGF0YS1kZWNrLWNvbGxlY3Rpb249XCIke2l0ZW0uZGVja0lkfVwiXWApO1xuICAgICAgICBpZiAoZWwpIHtcbiAgICAgICAgICBlbC50ZXh0Q29udGVudCA9IGBDb2xsZWN0aW9uOiAke2l0ZW0ucHJvZ3Jlc3MudG9GaXhlZCgyKX0lYDtcbiAgICAgICAgfVxuICAgICAgfSk7XG4gICAgfSlcbiAgICAuY2F0Y2goKCkgPT4ge30pO1xufVxuXG5leHBvcnQgZGVmYXVsdCAoKSA9PiB7XG4gIGRvY3VtZW50LmFkZEV2ZW50TGlzdGVuZXIoJ0RPTUNvbnRlbnRMb2FkZWQnLCBsb2FkRGVja0NvbGxlY3Rpb25Qcm9ncmVzcyk7XG4gIHdpbmRvdy5hZGRFdmVudExpc3RlbmVyKCdkZWNrLW92ZXJ2aWV3LXVwZGF0ZWQnLCBsb2FkRGVja0NvbGxlY3Rpb25Qcm9ncmVzcyk7XG59O1xuIiwiZXhwb3J0IGRlZmF1bHQgKGNvbmZpZzogQ29uZmlnKSA9PiAoe1xuXG4gICAgZmlsdGVyczogW10sXG5cbiAgICBpbml0KCkge1xuICAgICAgICB3aW5kb3cuYWRkRXZlbnRMaXN0ZW5lcihcIlNldEZpbHRlclwiLCAoZXZlbnQ6IEN1c3RvbUV2ZW50KSA9PiB7XG4gICAgICAgICAgICB0aGlzLnVwZGF0ZUZpbHRlckJ5VmFsdWVzKGV2ZW50LmRldGFpbC5uYW1lLCBldmVudC5kZXRhaWwudmFsdWUsIGV2ZW50LmRldGFpbC5vcHRpb24pO1xuICAgICAgICB9KTtcbiAgICAgICAgd2luZG93LmFkZEV2ZW50TGlzdGVuZXIoXCJSZXNldEZpbHRlcnNcIiwgKCkgPT4gdGhpcy5yZXNldEZpbHRlcnMoKSk7XG5cbiAgICB9LFxuXG4gICAgcmVnaXN0ZXJGaWx0ZXIoZWxlbWVudDogSFRNTElucHV0RWxlbWVudCkge1xuICAgICAgICB2YXIgZmlsdGVyOiBGaWx0ZXIgPSB0aGlzLmZpbHRlcnMuZmluZCgoZmlsdGVyOiBGaWx0ZXIpID0+IGZpbHRlci5rZXkgPT09IGVsZW1lbnQubmFtZSk7XG4gICAgICAgIGlmICghZmlsdGVyKSB7XG4gICAgICAgICAgICBmaWx0ZXIgPSB7XG4gICAgICAgICAgICAgICAga2V5OiBlbGVtZW50Lm5hbWUsXG4gICAgICAgICAgICAgICAgZmlsdGVySXRlbXM6IFtdXG4gICAgICAgICAgICB9O1xuICAgICAgICAgICAgdGhpcy5maWx0ZXJzLnB1c2goZmlsdGVyKTtcbiAgICAgICAgfVxuXG4gICAgICAgIGZpbHRlci5maWx0ZXJJdGVtcy5wdXNoKHtcbiAgICAgICAgICAgIHZhbHVlOiBlbGVtZW50LnZhbHVlLFxuICAgICAgICAgICAgb3B0aW9uOiBlbGVtZW50LmNoZWNrZWQgPyBJdGVtT3B0aW9uLkluY2x1ZGUgOiBJdGVtT3B0aW9uLk5vbmUsXG4gICAgICAgICAgICBlbGVtZW50OiBlbGVtZW50XG4gICAgICAgIH0pO1xuICAgIH0sXG5cbiAgICB1cGRhdGVGaWx0ZXIoZXZlbnQpIHtcbiAgICAgICAgdGhpcy51cGRhdGVGaWx0ZXJCeVRhcmdldChldmVudC50YXJnZXQpO1xuICAgIH0sXG5cbiAgICB1cGRhdGVGaWx0ZXJCeVRhcmdldCh0YXJnZXQpIHtcbiAgICAgICAgY29uc3QgZmlsdGVyTmFtZTogc3RyaW5nID0gdGFyZ2V0Lm5hbWU7XG4gICAgICAgIGNvbnN0IGZpbHRlclZhbHVlOiBzdHJpbmcgPSB0YXJnZXQudmFsdWU7XG4gICAgICAgIGNvbnN0IGZpbHRlckNoZWNrOiBib29sZWFuID0gdGFyZ2V0LmNoZWNrZWQ7XG5cbiAgICAgICAgdGhpcy51cGRhdGVGaWx0ZXJCeVZhbHVlcyhmaWx0ZXJOYW1lLCBmaWx0ZXJWYWx1ZSwgZmlsdGVyQ2hlY2sgPyBJdGVtT3B0aW9uLkluY2x1ZGUgOiBJdGVtT3B0aW9uLk5vbmUpO1xuICAgIH0sXG5cbiAgICB1cGRhdGVGaWx0ZXJCeVZhbHVlcyhuYW1lOiBzdHJpbmcsIHZhbHVlOiBzdHJpbmcsIG9wdGlvbjogSXRlbU9wdGlvbikge1xuICAgICAgICBjb25zdCBmaWx0ZXI6IEZpbHRlciA9IHRoaXMuZmlsdGVycy5maW5kKChpdGVtOiBGaWx0ZXIpID0+IGl0ZW0ua2V5ID09PSBuYW1lKTtcbiAgICAgICAgaWYgKCFmaWx0ZXIpIHsgcmV0dXJuOyB9XG5cbiAgICAgICAgY29uc3QgZmlsdGVySXRlbSA9IGZpbHRlci5maWx0ZXJJdGVtcy5maW5kKGl0ZW0gPT4gaXRlbS52YWx1ZSA9PT0gdmFsdWUpO1xuICAgICAgICBpZiAoIWZpbHRlckl0ZW0pIHsgcmV0dXJuOyB9XG5cbiAgICAgICAgaWYgKGZpbHRlckl0ZW0ub3B0aW9uID09PSBvcHRpb24pIHsgcmV0dXJuOyB9XG5cbiAgICAgICAgZmlsdGVySXRlbS5vcHRpb24gPSBvcHRpb247XG4gICAgICAgIGlmIChmaWx0ZXJJdGVtLmVsZW1lbnQpIHtcbiAgICAgICAgICAgIGlmIChmaWx0ZXJJdGVtLm9wdGlvbiA9PT0gSXRlbU9wdGlvbi5JbmNsdWRlKSB7XG4gICAgICAgICAgICAgICAgZmlsdGVySXRlbS5lbGVtZW50LmNoZWNrZWQgPSB0cnVlO1xuICAgICAgICAgICAgfSBlbHNlIGlmIChmaWx0ZXJJdGVtLm9wdGlvbiA9PT0gSXRlbU9wdGlvbi5Ob25lKSB7XG4gICAgICAgICAgICAgICAgZmlsdGVySXRlbS5lbGVtZW50LmNoZWNrZWQgPSBmYWxzZTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfVxuXG4gICAgICAgIHRoaXMudXBkYXRlVXJsKCk7XG4gICAgfSxcblxuICAgIGFkZEZpbHRlclZhbHVlKG5hbWU6IHN0cmluZywgdmFsdWU6IHN0cmluZylcbiAgICB7XG4gICAgICAgIHZhciBmaWx0ZXI6IEZpbHRlciA9IHRoaXMuZmlsdGVycy5maW5kKChmaWx0ZXI6IEZpbHRlcikgPT4gZmlsdGVyLmtleSA9PT0gbmFtZSk7XG4gICAgICAgIGlmICghZmlsdGVyKSB7XG4gICAgICAgICAgICBmaWx0ZXIgPSB7XG4gICAgICAgICAgICAgICAga2V5OiBuYW1lLFxuICAgICAgICAgICAgICAgIGZpbHRlckl0ZW1zOiBbXVxuICAgICAgICAgICAgfTtcbiAgICAgICAgICAgIHRoaXMuZmlsdGVycy5wdXNoKGZpbHRlcik7XG4gICAgICAgIH1cbiAgICAgICAgZmlsdGVyLmZpbHRlckl0ZW1zLnB1c2goe1xuICAgICAgICAgICAgdmFsdWU6IHZhbHVlLFxuICAgICAgICAgICAgb3B0aW9uOiBJdGVtT3B0aW9uLkluY2x1ZGVcbiAgICAgICAgfSk7XG4gICAgfSxcblxuICAgIHJlc2V0RmlsdGVycygpIHtcbiAgICAgICAgdGhpcy5maWx0ZXJzLmZvckVhY2goKGZpbHRlcjogRmlsdGVyKSA9PiB7XG4gICAgICAgICAgICBmaWx0ZXIuZmlsdGVySXRlbXMuZm9yRWFjaChpdGVtID0+IHtcbiAgICAgICAgICAgICAgICBpZiAoaXRlbS5vcHRpb24gPT09IEl0ZW1PcHRpb24uTm9uZSkgeyByZXR1cm4gZmFsc2U7IH1cbiAgICAgICAgICAgICAgICB0aGlzLnJlbW92ZUZpbHRlcihpdGVtKTtcbiAgICAgICAgICAgIH0pXG4gICAgICAgIH0pXG4gICAgfSxcblxuICAgIHJlbW92ZUZpbHRlcihmaWx0ZXJJdGVtOiBGaWx0ZXJJdGVtKSB7XG4gICAgICAgIGZpbHRlckl0ZW0ub3B0aW9uID0gSXRlbU9wdGlvbi5Ob25lO1xuICAgICAgICBpZiAoZmlsdGVySXRlbS5lbGVtZW50KSB7XG4gICAgICAgICAgICBmaWx0ZXJJdGVtLmVsZW1lbnQuY2hlY2tlZCA9IGZhbHNlO1xuICAgICAgICB9XG5cbiAgICAgICAgdGhpcy51cGRhdGVVcmwoKTtcbiAgICB9LFxuXG4gICAgdXBkYXRlVXJsKCkge1xuICAgICAgICBpZiAoIWNvbmZpZy5jaGFuZ2VVcmwpIHtcbiAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgfVxuXG4gICAgICAgIHZhciB1cmwgPSBuZXcgVVJMKHdpbmRvdy5sb2NhdGlvbi5ocmVmLnNwbGl0KCc/JylbMF0pO1xuXG4gICAgICAgIHRoaXMuZmlsdGVycy5mb3JFYWNoKChmaWx0ZXI6IEZpbHRlcikgPT4ge1xuICAgICAgICAgICAgY29uc3QgY2hlY2tlZEl0ZW1zID0gZmlsdGVyLmZpbHRlckl0ZW1zLmZpbHRlcihpdGVtID0+IGl0ZW0ub3B0aW9uID09PSBJdGVtT3B0aW9uLkluY2x1ZGUpO1xuICAgICAgICAgICAgaWYgKGNoZWNrZWRJdGVtcy5sZW5ndGggPT09IDApIHtcbiAgICAgICAgICAgICAgICB1cmwuc2VhcmNoUGFyYW1zLmRlbGV0ZShmaWx0ZXIua2V5KTtcbiAgICAgICAgICAgICAgICByZXR1cm47XG4gICAgICAgICAgICB9XG4gICAgICAgICAgICBjaGVja2VkSXRlbXMuZm9yRWFjaChpdGVtID0+IHtcbiAgICAgICAgICAgICAgICB1cmwuc2VhcmNoUGFyYW1zLmFwcGVuZChmaWx0ZXIua2V5LCBpdGVtLnZhbHVlKTtcbiAgICAgICAgICAgIH0pO1xuICAgICAgICB9KTtcblxuICAgICAgICBoaXN0b3J5LnJlcGxhY2VTdGF0ZSh7fSwgbnVsbCwgdXJsKTtcbiAgICB9XG59KTtcblxuaW50ZXJmYWNlIENvbmZpZyB7XG4gICAgY2hhbmdlVXJsOiBib29sZWFuO1xufVxuXG5pbnRlcmZhY2UgRmlsdGVyIHtcbiAgICBrZXk6IHN0cmluZyxcbiAgICBmaWx0ZXJJdGVtczogRmlsdGVySXRlbVtdXG59XG5cbmludGVyZmFjZSBGaWx0ZXJJdGVtIHtcbiAgICB2YWx1ZTogc3RyaW5nLFxuICAgIG9wdGlvbjogSXRlbU9wdGlvbixcbiAgICBlbGVtZW50PzogSFRNTElucHV0RWxlbWVudFxufVxuXG5pbnRlcmZhY2UgQ2FyZFZhbHVlIHtcbiAgICB0eXBlOiBzdHJpbmcsXG4gICAgdmFsdWU6IHN0cmluZyxcbiAgICB2YWx1ZXM6IHN0cmluZ1tdXG59XG5cbmVudW0gSXRlbU9wdGlvbiB7XG4gICAgTm9uZSxcbiAgICBJbmNsdWRlLFxuICAgIEV4Y2x1ZGVcbn0iLCJleHBvcnQgZGVmYXVsdCAoKSA9PiB7XG4gICAgZG9jdW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImNsaWNrXCIsIChldjogTW91c2VFdmVudCkgPT4ge1xuICAgICAgICBjb25zdCB0YXJnZXQgPSAoZXYudGFyZ2V0IGFzIEhUTUxFbGVtZW50KS5jbG9zZXN0KFwiLmpzLW9wZW4tbW9kYWxcIik7XG4gICAgICAgIGlmICghdGFyZ2V0KXtcbiAgICAgICAgICAgIHJldHVybjtcbiAgICAgICAgfVxuXG4gICAgICAgIGNvbnN0IG1vZGFsSWQgPSB0YXJnZXQuZ2V0QXR0cmlidXRlKFwianMtbW9kYWxcIik7XG4gICAgICAgIFxuICAgICAgICAoZG9jdW1lbnQuZ2V0RWxlbWVudEJ5SWQobW9kYWxJZCkgYXMgSFRNTERpYWxvZ0VsZW1lbnQpLnNob3dNb2RhbCgpO1xuXG4gICAgICAgIGV2LnByZXZlbnREZWZhdWx0KCk7XG4gICAgfSk7XG59IiwiZXhwb3J0IGRlZmF1bHQgKGNvbmZpZzogQ29uZmlnKSA9PiAoe1xuICBmaWx0ZXJzOiBbXSxcbiAgbG9hZGluZzogZmFsc2UsXG4gIHBhZ2VOdW1iZXI6IHVuZGVmaW5lZCxcblxuICBpbml0KCkge1xuICAgIHdpbmRvdy5hZGRFdmVudExpc3RlbmVyKFwiU2V0RmlsdGVyXCIsIChldmVudDogQ3VzdG9tRXZlbnQpID0+IHtcbiAgICAgIHRoaXMudXBkYXRlRmlsdGVyQnlWYWx1ZXMoXG4gICAgICAgIGV2ZW50LmRldGFpbC5uYW1lLFxuICAgICAgICBldmVudC5kZXRhaWwudmFsdWUsXG4gICAgICAgIGV2ZW50LmRldGFpbC5vcHRpb25cbiAgICAgICk7XG4gICAgfSk7XG4gICAgd2luZG93LmFkZEV2ZW50TGlzdGVuZXIoXCJSZXNldEZpbHRlcnNcIiwgKCkgPT4gdGhpcy5yZXNldEZpbHRlcnMoKSk7XG5cbiAgICB0aGlzLiRyZWZzLnN1Ym1pdEZvcm0uYWRkRXZlbnRMaXN0ZW5lcihcInN1Ym1pdFwiLCAoZXZlbnQ6IFN1Ym1pdEV2ZW50KSA9PiB7XG4gICAgICBldmVudC5wcmV2ZW50RGVmYXVsdCgpO1xuICAgICAgdGhpcy51cGRhdGVPdmVydmlldygpO1xuICAgIH0pO1xuICB9LFxuXG4gIHJlZ2lzdGVyRmlsdGVyKGVsZW1lbnQ6IEhUTUxJbnB1dEVsZW1lbnQsIG5hbWU6IHN0cmluZykge1xuICAgIHZhciBmaWx0ZXI6IEZpbHRlciA9IHRoaXMuZmlsdGVycy5maW5kKFxuICAgICAgKGZpbHRlcjogRmlsdGVyKSA9PiBmaWx0ZXIua2V5ID09PSBuYW1lXG4gICAgKTtcbiAgICBpZiAoIWZpbHRlcikge1xuICAgICAgZmlsdGVyID0ge1xuICAgICAgICBrZXk6IG5hbWUsXG4gICAgICAgIGZpbHRlckl0ZW1zOiBbXSxcbiAgICAgICAgZWxlbWVudExlc3M6IGZhbHNlLFxuICAgICAgfTtcbiAgICAgIHRoaXMuZmlsdGVycy5wdXNoKGZpbHRlcik7XG4gICAgfVxuXG4gICAgZmlsdGVyLmZpbHRlckl0ZW1zLnB1c2goe1xuICAgICAgdmFsdWU6IGVsZW1lbnQudmFsdWUsXG4gICAgICBsYWJlbDogZWxlbWVudC52YWx1ZSxcbiAgICAgIG9wdGlvbjogZWxlbWVudC5jaGVja2VkID8gSXRlbU9wdGlvbi5JbmNsdWRlIDogSXRlbU9wdGlvbi5Ob25lLFxuICAgICAgZWxlbWVudDogZWxlbWVudCxcbiAgICB9KTtcbiAgfSxcblxuICB1cGRhdGVGaWx0ZXIoZXZlbnQsIG5hbWU6IHN0cmluZykge1xuICAgIHRoaXMudXBkYXRlRmlsdGVyQnlUYXJnZXQoZXZlbnQudGFyZ2V0LCBuYW1lKTtcbiAgfSxcblxuICB1cGRhdGVGaWx0ZXJCeVRhcmdldCh0YXJnZXQsIG5hbWU6IHN0cmluZykge1xuICAgIGNvbnN0IGZpbHRlclZhbHVlOiBzdHJpbmcgPSB0YXJnZXQudmFsdWU7XG4gICAgY29uc3QgZmlsdGVyQ2hlY2s6IGJvb2xlYW4gPSB0YXJnZXQuY2hlY2tlZDtcblxuICAgIHRoaXMudXBkYXRlRmlsdGVyQnlWYWx1ZXMoXG4gICAgICBuYW1lLFxuICAgICAgZmlsdGVyVmFsdWUsXG4gICAgICBmaWx0ZXJDaGVjayA/IEl0ZW1PcHRpb24uSW5jbHVkZSA6IEl0ZW1PcHRpb24uTm9uZVxuICAgICk7XG4gIH0sXG5cbiAgdXBkYXRlRmlsdGVyQnlWYWx1ZXMobmFtZTogc3RyaW5nLCB2YWx1ZTogc3RyaW5nLCBvcHRpb246IEl0ZW1PcHRpb24pIHtcbiAgICBjb25zdCBmaWx0ZXI6IEZpbHRlciA9IHRoaXMuZmlsdGVycy5maW5kKFxuICAgICAgKGl0ZW06IEZpbHRlcikgPT4gaXRlbS5rZXkgPT09IG5hbWVcbiAgICApO1xuICAgIGlmICghZmlsdGVyKSB7XG4gICAgICByZXR1cm47XG4gICAgfVxuXG4gICAgY29uc3QgZmlsdGVySXRlbSA9IGZpbHRlci5maWx0ZXJJdGVtcy5maW5kKChpdGVtKSA9PiBpdGVtLnZhbHVlID09PSB2YWx1ZSk7XG4gICAgaWYgKCFmaWx0ZXJJdGVtKSB7XG4gICAgICByZXR1cm47XG4gICAgfVxuXG4gICAgaWYgKGZpbHRlckl0ZW0ub3B0aW9uID09PSBvcHRpb24pIHtcbiAgICAgIHJldHVybjtcbiAgICB9XG5cbiAgICBmaWx0ZXJJdGVtLm9wdGlvbiA9IG9wdGlvbjtcbiAgICBpZiAoZmlsdGVySXRlbS5lbGVtZW50KSB7XG4gICAgICBpZiAoZmlsdGVySXRlbS5vcHRpb24gPT09IEl0ZW1PcHRpb24uSW5jbHVkZSkge1xuICAgICAgICBmaWx0ZXJJdGVtLmVsZW1lbnQuY2hlY2tlZCA9IHRydWU7XG4gICAgICB9IGVsc2UgaWYgKGZpbHRlckl0ZW0ub3B0aW9uID09PSBJdGVtT3B0aW9uLk5vbmUpIHtcbiAgICAgICAgZmlsdGVySXRlbS5lbGVtZW50LmNoZWNrZWQgPSBmYWxzZTtcbiAgICAgIH1cbiAgICB9XG5cbiAgICB0aGlzLnJlc2V0UGFnZU51bWJlcigpO1xuICAgIHRoaXMudXBkYXRlVXJsKCk7XG4gICAgdGhpcy51cGRhdGVPdmVydmlldygpO1xuICB9LFxuXG4gIGFkZEZpbHRlclZhbHVlKG5hbWU6IHN0cmluZywgdmFsdWU6IHN0cmluZywgbGFiZWw6IHN0cmluZykge1xuICAgIHZhciBmaWx0ZXI6IEZpbHRlciA9IHRoaXMuZmlsdGVycy5maW5kKFxuICAgICAgKGZpbHRlcjogRmlsdGVyKSA9PiBmaWx0ZXIua2V5ID09PSBuYW1lXG4gICAgKTtcbiAgICBpZiAoIWZpbHRlcikge1xuICAgICAgZmlsdGVyID0ge1xuICAgICAgICBrZXk6IG5hbWUsXG4gICAgICAgIGZpbHRlckl0ZW1zOiBbXSxcbiAgICAgICAgZWxlbWVudExlc3M6IHRydWUsXG4gICAgICB9O1xuICAgICAgdGhpcy5maWx0ZXJzLnB1c2goZmlsdGVyKTtcbiAgICB9XG4gICAgZmlsdGVyLmZpbHRlckl0ZW1zLnB1c2goe1xuICAgICAgdmFsdWU6IHZhbHVlLFxuICAgICAgbGFiZWw6IGxhYmVsLFxuICAgICAgb3B0aW9uOiBJdGVtT3B0aW9uLkluY2x1ZGUsXG4gICAgfSk7XG5cbiAgICB0aGlzLnJlc2V0UGFnZU51bWJlcigpO1xuICAgIHRoaXMudXBkYXRlVXJsKCk7XG4gICAgdGhpcy51cGRhdGVPdmVydmlldygpO1xuICB9LFxuXG4gIG9uVXBkYXRlU2VhcmNoKCkge1xuICAgIHRoaXMucmVzZXRQYWdlTnVtYmVyKCk7XG4gICAgdGhpcy51cGRhdGVVcmwoKTtcbiAgfSxcblxuICBzZXRQYWdlTnVtYmVyKHBhZ2U6IG51bWJlcikge1xuICAgIHRoaXMucGFnZU51bWJlciA9IHBhZ2U7XG5cbiAgICB0aGlzLnVwZGF0ZVVybCgpO1xuICAgIHRoaXMudXBkYXRlT3ZlcnZpZXcoKTtcbiAgfSxcblxuICByZXNldFBhZ2VOdW1iZXIoKSB7XG4gICAgdGhpcy5wYWdlTnVtYmVyID0gdW5kZWZpbmVkO1xuICB9LFxuXG4gIHVwZGF0ZU92ZXJ2aWV3KCkge1xuICAgIGNvbnN0IHRhcmdldCA9IHRoaXMuJHJlZnMuc3VibWl0Rm9ybTtcbiAgICB2YXIgZGF0YSA9IG5ldyBGb3JtRGF0YSh0YXJnZXQpO1xuICAgIGRhdGEuYXBwZW5kKFwiaXNBamF4XCIsIFwidHJ1ZVwiKTtcbiAgICBpZiAodGhpcy5wYWdlTnVtYmVyKSB7XG4gICAgICBkYXRhLmFwcGVuZChcInBhZ2VOdW1iZXJcIiwgdGhpcy5wYWdlTnVtYmVyKTtcbiAgICB9XG5cbiAgICBjb25zdCBlbGVtZW50TGVzc0ZpbHRlcnMgPSB0aGlzLmZpbHRlcnMuZmlsdGVyKFxuICAgICAgKGl0ZW06IEZpbHRlcikgPT4gaXRlbS5lbGVtZW50TGVzc1xuICAgICkgYXMgRmlsdGVyW107XG4gICAgZWxlbWVudExlc3NGaWx0ZXJzLmZvckVhY2goKGZpbHRlcikgPT4ge1xuICAgICAgY29uc3QgaXRlbXMgPSBmaWx0ZXIuZmlsdGVySXRlbXMuZmlsdGVyKFxuICAgICAgICAoaXRlbSkgPT4gaXRlbS5vcHRpb24gPT09IEl0ZW1PcHRpb24uSW5jbHVkZVxuICAgICAgKTtcbiAgICAgIGlmIChpdGVtcy5sZW5ndGggPT09IDApIHJldHVybjtcblxuICAgICAgZGF0YS5hcHBlbmQoXG4gICAgICAgIGBGaWx0ZXJzWyR7ZmlsdGVyLmtleX1dYCxcbiAgICAgICAgaXRlbXMubWFwKChpdGVtKSA9PiBpdGVtLnZhbHVlKS5qb2luKFwiLFwiKVxuICAgICAgKTtcbiAgICB9KTtcblxuICAgIHRoaXMubG9hZGluZyA9IHRydWU7XG4gICAgZmV0Y2godGhpcy4kcmVmcy5zdWJtaXRGb3JtLmFjdGlvbiwge1xuICAgICAgbWV0aG9kOiBcIlBPU1RcIixcbiAgICAgIGJvZHk6IGRhdGEsXG4gICAgfSlcbiAgICAgIC50aGVuKChyZXMpID0+IHJlcy50ZXh0KCkpXG4gICAgICAudGhlbigocmVzKSA9PiB7XG4gICAgICAgIGNvbnN0IGNhcmRPdmVydmlld0VsZW0gPSBkb2N1bWVudC5nZXRFbGVtZW50QnlJZChcImNhcmQtb3ZlcnZpZXdcIik7XG5cbiAgICAgICAgY2FyZE92ZXJ2aWV3RWxlbS5pbm5lckhUTUwgPSByZXM7XG5cbiAgICAgICAgLy8gQHRzLWlnbm9yZVxuICAgICAgICBodG14LnByb2Nlc3MoY2FyZE92ZXJ2aWV3RWxlbSk7XG4gICAgICAgIHRoaXMubG9hZGluZyA9IGZhbHNlO1xuICAgICAgICB3aW5kb3cuZGlzcGF0Y2hFdmVudChuZXcgQ3VzdG9tRXZlbnQoJ2RlY2stb3ZlcnZpZXctdXBkYXRlZCcpKTtcbiAgICAgIH0pO1xuICB9LFxuXG4gIHJlc2V0RmlsdGVycygpIHtcbiAgICB0aGlzLmZpbHRlcnMuZm9yRWFjaCgoZmlsdGVyOiBGaWx0ZXIpID0+IHtcbiAgICAgIGZpbHRlci5maWx0ZXJJdGVtcy5mb3JFYWNoKChpdGVtKSA9PiB7XG4gICAgICAgIGlmIChpdGVtLm9wdGlvbiA9PT0gSXRlbU9wdGlvbi5Ob25lKSB7XG4gICAgICAgICAgcmV0dXJuIGZhbHNlO1xuICAgICAgICB9XG4gICAgICAgIHRoaXMucmVtb3ZlRmlsdGVyKGl0ZW0pO1xuICAgICAgfSk7XG4gICAgfSk7XG4gIH0sXG5cbiAgcmVtb3ZlRmlsdGVyKGZpbHRlckl0ZW06IEZpbHRlckl0ZW0pIHtcbiAgICBmaWx0ZXJJdGVtLm9wdGlvbiA9IEl0ZW1PcHRpb24uTm9uZTtcbiAgICBpZiAoZmlsdGVySXRlbS5lbGVtZW50KSB7XG4gICAgICBmaWx0ZXJJdGVtLmVsZW1lbnQuY2hlY2tlZCA9IGZhbHNlO1xuICAgIH1cblxuICAgIHRoaXMudXBkYXRlVXJsKCk7XG4gICAgdGhpcy51cGRhdGVPdmVydmlldygpO1xuICB9LFxuXG4gIHVwZGF0ZVVybCgpIHtcbiAgICBpZiAoIWNvbmZpZy5jaGFuZ2VVcmwpIHtcbiAgICAgIHJldHVybjtcbiAgICB9XG5cbiAgICB2YXIgdXJsID0gbmV3IFVSTCh3aW5kb3cubG9jYXRpb24uaHJlZi5zcGxpdChcIj9cIilbMF0pO1xuXG4gICAgdGhpcy5maWx0ZXJzLmZvckVhY2goKGZpbHRlcjogRmlsdGVyKSA9PiB7XG4gICAgICBjb25zdCBjaGVja2VkSXRlbXMgPSBmaWx0ZXIuZmlsdGVySXRlbXMuZmlsdGVyKFxuICAgICAgICAoaXRlbSkgPT4gaXRlbS5vcHRpb24gPT09IEl0ZW1PcHRpb24uSW5jbHVkZVxuICAgICAgKTtcbiAgICAgIGlmIChjaGVja2VkSXRlbXMubGVuZ3RoID09PSAwKSB7XG4gICAgICAgIHVybC5zZWFyY2hQYXJhbXMuZGVsZXRlKGZpbHRlci5rZXkpO1xuICAgICAgICByZXR1cm47XG4gICAgICB9XG4gICAgICBjaGVja2VkSXRlbXMuZm9yRWFjaCgoaXRlbSkgPT4ge1xuICAgICAgICB1cmwuc2VhcmNoUGFyYW1zLmFwcGVuZChmaWx0ZXIua2V5LCBpdGVtLnZhbHVlKTtcbiAgICAgIH0pO1xuICAgIH0pO1xuXG4gICAgaWYgKHRoaXMucGFnZU51bWJlcikge1xuICAgICAgdXJsLnNlYXJjaFBhcmFtcy5hcHBlbmQoXCJwYWdlXCIsIHRoaXMucGFnZU51bWJlcik7XG4gICAgfVxuXG4gICAgaGlzdG9yeS5yZXBsYWNlU3RhdGUoe30sIG51bGwsIHVybCk7XG4gIH0sXG59KTtcblxuaW50ZXJmYWNlIENvbmZpZyB7XG4gIGNoYW5nZVVybDogYm9vbGVhbjtcbn1cblxuaW50ZXJmYWNlIEZpbHRlciB7XG4gIGtleTogc3RyaW5nO1xuICBmaWx0ZXJJdGVtczogRmlsdGVySXRlbVtdO1xuICBlbGVtZW50TGVzczogYm9vbGVhbjtcbn1cblxuaW50ZXJmYWNlIEZpbHRlckl0ZW0ge1xuICB2YWx1ZTogc3RyaW5nO1xuICBsYWJlbDogc3RyaW5nO1xuICBvcHRpb246IEl0ZW1PcHRpb247XG4gIGVsZW1lbnQ/OiBIVE1MSW5wdXRFbGVtZW50O1xufVxuXG5pbnRlcmZhY2UgQ2FyZFZhbHVlIHtcbiAgdHlwZTogc3RyaW5nO1xuICB2YWx1ZTogc3RyaW5nO1xuICB2YWx1ZXM6IHN0cmluZ1tdO1xufVxuXG5lbnVtIEl0ZW1PcHRpb24ge1xuICBOb25lLFxuICBJbmNsdWRlLFxuICBFeGNsdWRlLFxufVxuIiwiZXhwb3J0IGRlZmF1bHQgKCkgPT4ge1xuICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIudG9vbHRpcC1zdGFydGVyXCIpLmZvckVhY2goKGVsZW1lbnQ6IEhUTUxFbGVtZW50KSA9PiB7XG4gICAgICAgIGNvbnN0IHRvb2x0aXBzID0gZWxlbWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiLnRvb2x0aXBcIik7XG4gICAgICAgIGNvbnN0IHBhcmVudEJvdW5kaW5nID0gZWxlbWVudC5nZXRCb3VuZGluZ0NsaWVudFJlY3QoKTtcblxuICAgICAgICB0b29sdGlwcy5mb3JFYWNoKGl0ZW0gPT4ge1xuICAgICAgICAgICAgY29uc3QgYm90dG9tWFRvb2x0aXAgPSAzMDAgKyBwYXJlbnRCb3VuZGluZy55O1xuICAgICAgICAgICAgaWYgKGJvdHRvbVhUb29sdGlwID49IHdpbmRvdy5pbm5lckhlaWdodCkge1xuICAgICAgICAgICAgICAgIGl0ZW0uY2xhc3NMaXN0LmFkZChcInRvcFwiKTtcbiAgICAgICAgICAgIH1cbiAgICAgICAgfSlcbiAgICAgICAgZWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwibW91c2VlbnRlclwiLCAoKSA9PiB7XG4gICAgICAgICAgICB0b29sdGlwcy5mb3JFYWNoKGl0ZW0gPT4ge1xuICAgICAgICAgICAgICAgIGl0ZW0uY2xhc3NMaXN0LmFkZChcInNob3duXCIpO1xuICAgICAgICAgICAgfSk7XG4gICAgICAgIH0pO1xuXG4gICAgICAgIGVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcIm1vdXNlbGVhdmVcIiwgKCkgPT4ge1xuICAgICAgICAgICAgdG9vbHRpcHMuZm9yRWFjaChpdGVtID0+IGl0ZW0uY2xhc3NMaXN0LnJlbW92ZShcInNob3duXCIpKTtcbiAgICAgICAgfSlcbiAgICB9KTtcblxuICAgIGRvY3VtZW50LnF1ZXJ5U2VsZWN0b3JBbGwoXCIudG9vbHRpcC1jb250YWluZXJcIikuZm9yRWFjaCgoZWxlbWVudDogSFRNTEVsZW1lbnQpID0+IHtcbiAgICAgICAgZWxlbWVudC5hZGRFdmVudExpc3RlbmVyKFwibW91c2VlbnRlclwiLCAoZXZlbnQ6IE1vdXNlRXZlbnQpID0+IHtcbiAgICAgICAgICAgIGNvbnNvbGUubG9nKGV2ZW50KTtcbiAgICAgICAgfSwgdHJ1ZSk7XG4gICAgfSk7XG59IiwiZXhwb3J0IGRlZmF1bHQgKCkgPT4ge1xuICAgIGNvbnN0IHRvZ2dsZXJzID0gQXJyYXkuZnJvbTxIVE1MSW5wdXRFbGVtZW50Pihkb2N1bWVudC5xdWVyeVNlbGVjdG9yQWxsKFwiW2RhdGEtdG9nZ2xlXVwiKSk7XG4gICAgdG9nZ2xlcnMuZm9yRWFjaCgoZWxlbWVudCkgPT4ge1xuICAgICAgICBjb25zdCB0b2dnbGVFbGVtZW50cyA9IGVsZW1lbnQuZGF0YXNldC50b2dnbGU7XG4gICAgICAgIGVsZW1lbnQuYWRkRXZlbnRMaXN0ZW5lcihcImlucHV0XCIsICgpID0+IHtcbiAgICAgICAgICAgIGNvbnN0IHRvZ2dsZUVsZW1lbnRzID0gZG9jdW1lbnQucXVlcnlTZWxlY3RvckFsbDxIVE1MRWxlbWVudD4oZWxlbWVudC5kYXRhc2V0LnRvZ2dsZSk7XG4gICAgICAgICAgICB0b2dnbGVFbGVtZW50cy5mb3JFYWNoKChlbCkgPT4gZWwuc3R5bGUuZGlzcGxheSA9IGVsLnN0eWxlLmRpc3BsYXkgPT0gJ25vbmUnID8gJycgOiAnbm9uZScpO1xuICAgICAgICB9KVxuICAgIH0pXG59IiwiaW1wb3J0IHsgZ2V0U2lkZUF4aXMsIGdldEFsaWdubWVudEF4aXMsIGdldEF4aXNMZW5ndGgsIGdldFNpZGUsIGdldEFsaWdubWVudCwgZXZhbHVhdGUsIGdldFBhZGRpbmdPYmplY3QsIHJlY3RUb0NsaWVudFJlY3QsIG1pbiwgY2xhbXAsIHBsYWNlbWVudHMsIGdldEFsaWdubWVudFNpZGVzLCBnZXRPcHBvc2l0ZUFsaWdubWVudFBsYWNlbWVudCwgZ2V0T3Bwb3NpdGVQbGFjZW1lbnQsIGdldEV4cGFuZGVkUGxhY2VtZW50cywgZ2V0T3Bwb3NpdGVBeGlzUGxhY2VtZW50cywgc2lkZXMsIG1heCwgZ2V0T3Bwb3NpdGVBeGlzIH0gZnJvbSAnQGZsb2F0aW5nLXVpL3V0aWxzJztcbmV4cG9ydCB7IHJlY3RUb0NsaWVudFJlY3QgfSBmcm9tICdAZmxvYXRpbmctdWkvdXRpbHMnO1xuXG5mdW5jdGlvbiBjb21wdXRlQ29vcmRzRnJvbVBsYWNlbWVudChfcmVmLCBwbGFjZW1lbnQsIHJ0bCkge1xuICBsZXQge1xuICAgIHJlZmVyZW5jZSxcbiAgICBmbG9hdGluZ1xuICB9ID0gX3JlZjtcbiAgY29uc3Qgc2lkZUF4aXMgPSBnZXRTaWRlQXhpcyhwbGFjZW1lbnQpO1xuICBjb25zdCBhbGlnbm1lbnRBeGlzID0gZ2V0QWxpZ25tZW50QXhpcyhwbGFjZW1lbnQpO1xuICBjb25zdCBhbGlnbkxlbmd0aCA9IGdldEF4aXNMZW5ndGgoYWxpZ25tZW50QXhpcyk7XG4gIGNvbnN0IHNpZGUgPSBnZXRTaWRlKHBsYWNlbWVudCk7XG4gIGNvbnN0IGlzVmVydGljYWwgPSBzaWRlQXhpcyA9PT0gJ3knO1xuICBjb25zdCBjb21tb25YID0gcmVmZXJlbmNlLnggKyByZWZlcmVuY2Uud2lkdGggLyAyIC0gZmxvYXRpbmcud2lkdGggLyAyO1xuICBjb25zdCBjb21tb25ZID0gcmVmZXJlbmNlLnkgKyByZWZlcmVuY2UuaGVpZ2h0IC8gMiAtIGZsb2F0aW5nLmhlaWdodCAvIDI7XG4gIGNvbnN0IGNvbW1vbkFsaWduID0gcmVmZXJlbmNlW2FsaWduTGVuZ3RoXSAvIDIgLSBmbG9hdGluZ1thbGlnbkxlbmd0aF0gLyAyO1xuICBsZXQgY29vcmRzO1xuICBzd2l0Y2ggKHNpZGUpIHtcbiAgICBjYXNlICd0b3AnOlxuICAgICAgY29vcmRzID0ge1xuICAgICAgICB4OiBjb21tb25YLFxuICAgICAgICB5OiByZWZlcmVuY2UueSAtIGZsb2F0aW5nLmhlaWdodFxuICAgICAgfTtcbiAgICAgIGJyZWFrO1xuICAgIGNhc2UgJ2JvdHRvbSc6XG4gICAgICBjb29yZHMgPSB7XG4gICAgICAgIHg6IGNvbW1vblgsXG4gICAgICAgIHk6IHJlZmVyZW5jZS55ICsgcmVmZXJlbmNlLmhlaWdodFxuICAgICAgfTtcbiAgICAgIGJyZWFrO1xuICAgIGNhc2UgJ3JpZ2h0JzpcbiAgICAgIGNvb3JkcyA9IHtcbiAgICAgICAgeDogcmVmZXJlbmNlLnggKyByZWZlcmVuY2Uud2lkdGgsXG4gICAgICAgIHk6IGNvbW1vbllcbiAgICAgIH07XG4gICAgICBicmVhaztcbiAgICBjYXNlICdsZWZ0JzpcbiAgICAgIGNvb3JkcyA9IHtcbiAgICAgICAgeDogcmVmZXJlbmNlLnggLSBmbG9hdGluZy53aWR0aCxcbiAgICAgICAgeTogY29tbW9uWVxuICAgICAgfTtcbiAgICAgIGJyZWFrO1xuICAgIGRlZmF1bHQ6XG4gICAgICBjb29yZHMgPSB7XG4gICAgICAgIHg6IHJlZmVyZW5jZS54LFxuICAgICAgICB5OiByZWZlcmVuY2UueVxuICAgICAgfTtcbiAgfVxuICBzd2l0Y2ggKGdldEFsaWdubWVudChwbGFjZW1lbnQpKSB7XG4gICAgY2FzZSAnc3RhcnQnOlxuICAgICAgY29vcmRzW2FsaWdubWVudEF4aXNdIC09IGNvbW1vbkFsaWduICogKHJ0bCAmJiBpc1ZlcnRpY2FsID8gLTEgOiAxKTtcbiAgICAgIGJyZWFrO1xuICAgIGNhc2UgJ2VuZCc6XG4gICAgICBjb29yZHNbYWxpZ25tZW50QXhpc10gKz0gY29tbW9uQWxpZ24gKiAocnRsICYmIGlzVmVydGljYWwgPyAtMSA6IDEpO1xuICAgICAgYnJlYWs7XG4gIH1cbiAgcmV0dXJuIGNvb3Jkcztcbn1cblxuLyoqXG4gKiBDb21wdXRlcyB0aGUgYHhgIGFuZCBgeWAgY29vcmRpbmF0ZXMgdGhhdCB3aWxsIHBsYWNlIHRoZSBmbG9hdGluZyBlbGVtZW50XG4gKiBuZXh0IHRvIGEgZ2l2ZW4gcmVmZXJlbmNlIGVsZW1lbnQuXG4gKlxuICogVGhpcyBleHBvcnQgZG9lcyBub3QgaGF2ZSBhbnkgYHBsYXRmb3JtYCBpbnRlcmZhY2UgbG9naWMuIFlvdSB3aWxsIG5lZWQgdG9cbiAqIHdyaXRlIG9uZSBmb3IgdGhlIHBsYXRmb3JtIHlvdSBhcmUgdXNpbmcgRmxvYXRpbmcgVUkgd2l0aC5cbiAqL1xuY29uc3QgY29tcHV0ZVBvc2l0aW9uID0gYXN5bmMgKHJlZmVyZW5jZSwgZmxvYXRpbmcsIGNvbmZpZykgPT4ge1xuICBjb25zdCB7XG4gICAgcGxhY2VtZW50ID0gJ2JvdHRvbScsXG4gICAgc3RyYXRlZ3kgPSAnYWJzb2x1dGUnLFxuICAgIG1pZGRsZXdhcmUgPSBbXSxcbiAgICBwbGF0Zm9ybVxuICB9ID0gY29uZmlnO1xuICBjb25zdCB2YWxpZE1pZGRsZXdhcmUgPSBtaWRkbGV3YXJlLmZpbHRlcihCb29sZWFuKTtcbiAgY29uc3QgcnRsID0gYXdhaXQgKHBsYXRmb3JtLmlzUlRMID09IG51bGwgPyB2b2lkIDAgOiBwbGF0Zm9ybS5pc1JUTChmbG9hdGluZykpO1xuICBsZXQgcmVjdHMgPSBhd2FpdCBwbGF0Zm9ybS5nZXRFbGVtZW50UmVjdHMoe1xuICAgIHJlZmVyZW5jZSxcbiAgICBmbG9hdGluZyxcbiAgICBzdHJhdGVneVxuICB9KTtcbiAgbGV0IHtcbiAgICB4LFxuICAgIHlcbiAgfSA9IGNvbXB1dGVDb29yZHNGcm9tUGxhY2VtZW50KHJlY3RzLCBwbGFjZW1lbnQsIHJ0bCk7XG4gIGxldCBzdGF0ZWZ1bFBsYWNlbWVudCA9IHBsYWNlbWVudDtcbiAgbGV0IG1pZGRsZXdhcmVEYXRhID0ge307XG4gIGxldCByZXNldENvdW50ID0gMDtcbiAgZm9yIChsZXQgaSA9IDA7IGkgPCB2YWxpZE1pZGRsZXdhcmUubGVuZ3RoOyBpKyspIHtcbiAgICBjb25zdCB7XG4gICAgICBuYW1lLFxuICAgICAgZm5cbiAgICB9ID0gdmFsaWRNaWRkbGV3YXJlW2ldO1xuICAgIGNvbnN0IHtcbiAgICAgIHg6IG5leHRYLFxuICAgICAgeTogbmV4dFksXG4gICAgICBkYXRhLFxuICAgICAgcmVzZXRcbiAgICB9ID0gYXdhaXQgZm4oe1xuICAgICAgeCxcbiAgICAgIHksXG4gICAgICBpbml0aWFsUGxhY2VtZW50OiBwbGFjZW1lbnQsXG4gICAgICBwbGFjZW1lbnQ6IHN0YXRlZnVsUGxhY2VtZW50LFxuICAgICAgc3RyYXRlZ3ksXG4gICAgICBtaWRkbGV3YXJlRGF0YSxcbiAgICAgIHJlY3RzLFxuICAgICAgcGxhdGZvcm0sXG4gICAgICBlbGVtZW50czoge1xuICAgICAgICByZWZlcmVuY2UsXG4gICAgICAgIGZsb2F0aW5nXG4gICAgICB9XG4gICAgfSk7XG4gICAgeCA9IG5leHRYICE9IG51bGwgPyBuZXh0WCA6IHg7XG4gICAgeSA9IG5leHRZICE9IG51bGwgPyBuZXh0WSA6IHk7XG4gICAgbWlkZGxld2FyZURhdGEgPSB7XG4gICAgICAuLi5taWRkbGV3YXJlRGF0YSxcbiAgICAgIFtuYW1lXToge1xuICAgICAgICAuLi5taWRkbGV3YXJlRGF0YVtuYW1lXSxcbiAgICAgICAgLi4uZGF0YVxuICAgICAgfVxuICAgIH07XG4gICAgaWYgKHJlc2V0ICYmIHJlc2V0Q291bnQgPD0gNTApIHtcbiAgICAgIHJlc2V0Q291bnQrKztcbiAgICAgIGlmICh0eXBlb2YgcmVzZXQgPT09ICdvYmplY3QnKSB7XG4gICAgICAgIGlmIChyZXNldC5wbGFjZW1lbnQpIHtcbiAgICAgICAgICBzdGF0ZWZ1bFBsYWNlbWVudCA9IHJlc2V0LnBsYWNlbWVudDtcbiAgICAgICAgfVxuICAgICAgICBpZiAocmVzZXQucmVjdHMpIHtcbiAgICAgICAgICByZWN0cyA9IHJlc2V0LnJlY3RzID09PSB0cnVlID8gYXdhaXQgcGxhdGZvcm0uZ2V0RWxlbWVudFJlY3RzKHtcbiAgICAgICAgICAgIHJlZmVyZW5jZSxcbiAgICAgICAgICAgIGZsb2F0aW5nLFxuICAgICAgICAgICAgc3RyYXRlZ3lcbiAgICAgICAgICB9KSA6IHJlc2V0LnJlY3RzO1xuICAgICAgICB9XG4gICAgICAgICh7XG4gICAgICAgICAgeCxcbiAgICAgICAgICB5XG4gICAgICAgIH0gPSBjb21wdXRlQ29vcmRzRnJvbVBsYWNlbWVudChyZWN0cywgc3RhdGVmdWxQbGFjZW1lbnQsIHJ0bCkpO1xuICAgICAgfVxuICAgICAgaSA9IC0xO1xuICAgIH1cbiAgfVxuICByZXR1cm4ge1xuICAgIHgsXG4gICAgeSxcbiAgICBwbGFjZW1lbnQ6IHN0YXRlZnVsUGxhY2VtZW50LFxuICAgIHN0cmF0ZWd5LFxuICAgIG1pZGRsZXdhcmVEYXRhXG4gIH07XG59O1xuXG4vKipcbiAqIFJlc29sdmVzIHdpdGggYW4gb2JqZWN0IG9mIG92ZXJmbG93IHNpZGUgb2Zmc2V0cyB0aGF0IGRldGVybWluZSBob3cgbXVjaCB0aGVcbiAqIGVsZW1lbnQgaXMgb3ZlcmZsb3dpbmcgYSBnaXZlbiBjbGlwcGluZyBib3VuZGFyeSBvbiBlYWNoIHNpZGUuXG4gKiAtIHBvc2l0aXZlID0gb3ZlcmZsb3dpbmcgdGhlIGJvdW5kYXJ5IGJ5IHRoYXQgbnVtYmVyIG9mIHBpeGVsc1xuICogLSBuZWdhdGl2ZSA9IGhvdyBtYW55IHBpeGVscyBsZWZ0IGJlZm9yZSBpdCB3aWxsIG92ZXJmbG93XG4gKiAtIDAgPSBsaWVzIGZsdXNoIHdpdGggdGhlIGJvdW5kYXJ5XG4gKiBAc2VlIGh0dHBzOi8vZmxvYXRpbmctdWkuY29tL2RvY3MvZGV0ZWN0T3ZlcmZsb3dcbiAqL1xuYXN5bmMgZnVuY3Rpb24gZGV0ZWN0T3ZlcmZsb3coc3RhdGUsIG9wdGlvbnMpIHtcbiAgdmFyIF9hd2FpdCRwbGF0Zm9ybSRpc0VsZTtcbiAgaWYgKG9wdGlvbnMgPT09IHZvaWQgMCkge1xuICAgIG9wdGlvbnMgPSB7fTtcbiAgfVxuICBjb25zdCB7XG4gICAgeCxcbiAgICB5LFxuICAgIHBsYXRmb3JtLFxuICAgIHJlY3RzLFxuICAgIGVsZW1lbnRzLFxuICAgIHN0cmF0ZWd5XG4gIH0gPSBzdGF0ZTtcbiAgY29uc3Qge1xuICAgIGJvdW5kYXJ5ID0gJ2NsaXBwaW5nQW5jZXN0b3JzJyxcbiAgICByb290Qm91bmRhcnkgPSAndmlld3BvcnQnLFxuICAgIGVsZW1lbnRDb250ZXh0ID0gJ2Zsb2F0aW5nJyxcbiAgICBhbHRCb3VuZGFyeSA9IGZhbHNlLFxuICAgIHBhZGRpbmcgPSAwXG4gIH0gPSBldmFsdWF0ZShvcHRpb25zLCBzdGF0ZSk7XG4gIGNvbnN0IHBhZGRpbmdPYmplY3QgPSBnZXRQYWRkaW5nT2JqZWN0KHBhZGRpbmcpO1xuICBjb25zdCBhbHRDb250ZXh0ID0gZWxlbWVudENvbnRleHQgPT09ICdmbG9hdGluZycgPyAncmVmZXJlbmNlJyA6ICdmbG9hdGluZyc7XG4gIGNvbnN0IGVsZW1lbnQgPSBlbGVtZW50c1thbHRCb3VuZGFyeSA/IGFsdENvbnRleHQgOiBlbGVtZW50Q29udGV4dF07XG4gIGNvbnN0IGNsaXBwaW5nQ2xpZW50UmVjdCA9IHJlY3RUb0NsaWVudFJlY3QoYXdhaXQgcGxhdGZvcm0uZ2V0Q2xpcHBpbmdSZWN0KHtcbiAgICBlbGVtZW50OiAoKF9hd2FpdCRwbGF0Zm9ybSRpc0VsZSA9IGF3YWl0IChwbGF0Zm9ybS5pc0VsZW1lbnQgPT0gbnVsbCA/IHZvaWQgMCA6IHBsYXRmb3JtLmlzRWxlbWVudChlbGVtZW50KSkpICE9IG51bGwgPyBfYXdhaXQkcGxhdGZvcm0kaXNFbGUgOiB0cnVlKSA/IGVsZW1lbnQgOiBlbGVtZW50LmNvbnRleHRFbGVtZW50IHx8IChhd2FpdCAocGxhdGZvcm0uZ2V0RG9jdW1lbnRFbGVtZW50ID09IG51bGwgPyB2b2lkIDAgOiBwbGF0Zm9ybS5nZXREb2N1bWVudEVsZW1lbnQoZWxlbWVudHMuZmxvYXRpbmcpKSksXG4gICAgYm91bmRhcnksXG4gICAgcm9vdEJvdW5kYXJ5LFxuICAgIHN0cmF0ZWd5XG4gIH0pKTtcbiAgY29uc3QgcmVjdCA9IGVsZW1lbnRDb250ZXh0ID09PSAnZmxvYXRpbmcnID8ge1xuICAgIHgsXG4gICAgeSxcbiAgICB3aWR0aDogcmVjdHMuZmxvYXRpbmcud2lkdGgsXG4gICAgaGVpZ2h0OiByZWN0cy5mbG9hdGluZy5oZWlnaHRcbiAgfSA6IHJlY3RzLnJlZmVyZW5jZTtcbiAgY29uc3Qgb2Zmc2V0UGFyZW50ID0gYXdhaXQgKHBsYXRmb3JtLmdldE9mZnNldFBhcmVudCA9PSBudWxsID8gdm9pZCAwIDogcGxhdGZvcm0uZ2V0T2Zmc2V0UGFyZW50KGVsZW1lbnRzLmZsb2F0aW5nKSk7XG4gIGNvbnN0IG9mZnNldFNjYWxlID0gKGF3YWl0IChwbGF0Zm9ybS5pc0VsZW1lbnQgPT0gbnVsbCA/IHZvaWQgMCA6IHBsYXRmb3JtLmlzRWxlbWVudChvZmZzZXRQYXJlbnQpKSkgPyAoYXdhaXQgKHBsYXRmb3JtLmdldFNjYWxlID09IG51bGwgPyB2b2lkIDAgOiBwbGF0Zm9ybS5nZXRTY2FsZShvZmZzZXRQYXJlbnQpKSkgfHwge1xuICAgIHg6IDEsXG4gICAgeTogMVxuICB9IDoge1xuICAgIHg6IDEsXG4gICAgeTogMVxuICB9O1xuICBjb25zdCBlbGVtZW50Q2xpZW50UmVjdCA9IHJlY3RUb0NsaWVudFJlY3QocGxhdGZvcm0uY29udmVydE9mZnNldFBhcmVudFJlbGF0aXZlUmVjdFRvVmlld3BvcnRSZWxhdGl2ZVJlY3QgPyBhd2FpdCBwbGF0Zm9ybS5jb252ZXJ0T2Zmc2V0UGFyZW50UmVsYXRpdmVSZWN0VG9WaWV3cG9ydFJlbGF0aXZlUmVjdCh7XG4gICAgZWxlbWVudHMsXG4gICAgcmVjdCxcbiAgICBvZmZzZXRQYXJlbnQsXG4gICAgc3RyYXRlZ3lcbiAgfSkgOiByZWN0KTtcbiAgcmV0dXJuIHtcbiAgICB0b3A6IChjbGlwcGluZ0NsaWVudFJlY3QudG9wIC0gZWxlbWVudENsaWVudFJlY3QudG9wICsgcGFkZGluZ09iamVjdC50b3ApIC8gb2Zmc2V0U2NhbGUueSxcbiAgICBib3R0b206IChlbGVtZW50Q2xpZW50UmVjdC5ib3R0b20gLSBjbGlwcGluZ0NsaWVudFJlY3QuYm90dG9tICsgcGFkZGluZ09iamVjdC5ib3R0b20pIC8gb2Zmc2V0U2NhbGUueSxcbiAgICBsZWZ0OiAoY2xpcHBpbmdDbGllbnRSZWN0LmxlZnQgLSBlbGVtZW50Q2xpZW50UmVjdC5sZWZ0ICsgcGFkZGluZ09iamVjdC5sZWZ0KSAvIG9mZnNldFNjYWxlLngsXG4gICAgcmlnaHQ6IChlbGVtZW50Q2xpZW50UmVjdC5yaWdodCAtIGNsaXBwaW5nQ2xpZW50UmVjdC5yaWdodCArIHBhZGRpbmdPYmplY3QucmlnaHQpIC8gb2Zmc2V0U2NhbGUueFxuICB9O1xufVxuXG4vKipcbiAqIFByb3ZpZGVzIGRhdGEgdG8gcG9zaXRpb24gYW4gaW5uZXIgZWxlbWVudCBvZiB0aGUgZmxvYXRpbmcgZWxlbWVudCBzbyB0aGF0IGl0XG4gKiBhcHBlYXJzIGNlbnRlcmVkIHRvIHRoZSByZWZlcmVuY2UgZWxlbWVudC5cbiAqIEBzZWUgaHR0cHM6Ly9mbG9hdGluZy11aS5jb20vZG9jcy9hcnJvd1xuICovXG5jb25zdCBhcnJvdyA9IG9wdGlvbnMgPT4gKHtcbiAgbmFtZTogJ2Fycm93JyxcbiAgb3B0aW9ucyxcbiAgYXN5bmMgZm4oc3RhdGUpIHtcbiAgICBjb25zdCB7XG4gICAgICB4LFxuICAgICAgeSxcbiAgICAgIHBsYWNlbWVudCxcbiAgICAgIHJlY3RzLFxuICAgICAgcGxhdGZvcm0sXG4gICAgICBlbGVtZW50cyxcbiAgICAgIG1pZGRsZXdhcmVEYXRhXG4gICAgfSA9IHN0YXRlO1xuICAgIC8vIFNpbmNlIGBlbGVtZW50YCBpcyByZXF1aXJlZCwgd2UgZG9uJ3QgUGFydGlhbDw+IHRoZSB0eXBlLlxuICAgIGNvbnN0IHtcbiAgICAgIGVsZW1lbnQsXG4gICAgICBwYWRkaW5nID0gMFxuICAgIH0gPSBldmFsdWF0ZShvcHRpb25zLCBzdGF0ZSkgfHwge307XG4gICAgaWYgKGVsZW1lbnQgPT0gbnVsbCkge1xuICAgICAgcmV0dXJuIHt9O1xuICAgIH1cbiAgICBjb25zdCBwYWRkaW5nT2JqZWN0ID0gZ2V0UGFkZGluZ09iamVjdChwYWRkaW5nKTtcbiAgICBjb25zdCBjb29yZHMgPSB7XG4gICAgICB4LFxuICAgICAgeVxuICAgIH07XG4gICAgY29uc3QgYXhpcyA9IGdldEFsaWdubWVudEF4aXMocGxhY2VtZW50KTtcbiAgICBjb25zdCBsZW5ndGggPSBnZXRBeGlzTGVuZ3RoKGF4aXMpO1xuICAgIGNvbnN0IGFycm93RGltZW5zaW9ucyA9IGF3YWl0IHBsYXRmb3JtLmdldERpbWVuc2lvbnMoZWxlbWVudCk7XG4gICAgY29uc3QgaXNZQXhpcyA9IGF4aXMgPT09ICd5JztcbiAgICBjb25zdCBtaW5Qcm9wID0gaXNZQXhpcyA/ICd0b3AnIDogJ2xlZnQnO1xuICAgIGNvbnN0IG1heFByb3AgPSBpc1lBeGlzID8gJ2JvdHRvbScgOiAncmlnaHQnO1xuICAgIGNvbnN0IGNsaWVudFByb3AgPSBpc1lBeGlzID8gJ2NsaWVudEhlaWdodCcgOiAnY2xpZW50V2lkdGgnO1xuICAgIGNvbnN0IGVuZERpZmYgPSByZWN0cy5yZWZlcmVuY2VbbGVuZ3RoXSArIHJlY3RzLnJlZmVyZW5jZVtheGlzXSAtIGNvb3Jkc1theGlzXSAtIHJlY3RzLmZsb2F0aW5nW2xlbmd0aF07XG4gICAgY29uc3Qgc3RhcnREaWZmID0gY29vcmRzW2F4aXNdIC0gcmVjdHMucmVmZXJlbmNlW2F4aXNdO1xuICAgIGNvbnN0IGFycm93T2Zmc2V0UGFyZW50ID0gYXdhaXQgKHBsYXRmb3JtLmdldE9mZnNldFBhcmVudCA9PSBudWxsID8gdm9pZCAwIDogcGxhdGZvcm0uZ2V0T2Zmc2V0UGFyZW50KGVsZW1lbnQpKTtcbiAgICBsZXQgY2xpZW50U2l6ZSA9IGFycm93T2Zmc2V0UGFyZW50ID8gYXJyb3dPZmZzZXRQYXJlbnRbY2xpZW50UHJvcF0gOiAwO1xuXG4gICAgLy8gRE9NIHBsYXRmb3JtIGNhbiByZXR1cm4gYHdpbmRvd2AgYXMgdGhlIGBvZmZzZXRQYXJlbnRgLlxuICAgIGlmICghY2xpZW50U2l6ZSB8fCAhKGF3YWl0IChwbGF0Zm9ybS5pc0VsZW1lbnQgPT0gbnVsbCA/IHZvaWQgMCA6IHBsYXRmb3JtLmlzRWxlbWVudChhcnJvd09mZnNldFBhcmVudCkpKSkge1xuICAgICAgY2xpZW50U2l6ZSA9IGVsZW1lbnRzLmZsb2F0aW5nW2NsaWVudFByb3BdIHx8IHJlY3RzLmZsb2F0aW5nW2xlbmd0aF07XG4gICAgfVxuICAgIGNvbnN0IGNlbnRlclRvUmVmZXJlbmNlID0gZW5kRGlmZiAvIDIgLSBzdGFydERpZmYgLyAyO1xuXG4gICAgLy8gSWYgdGhlIHBhZGRpbmcgaXMgbGFyZ2UgZW5vdWdoIHRoYXQgaXQgY2F1c2VzIHRoZSBhcnJvdyB0byBubyBsb25nZXIgYmVcbiAgICAvLyBjZW50ZXJlZCwgbW9kaWZ5IHRoZSBwYWRkaW5nIHNvIHRoYXQgaXQgaXMgY2VudGVyZWQuXG4gICAgY29uc3QgbGFyZ2VzdFBvc3NpYmxlUGFkZGluZyA9IGNsaWVudFNpemUgLyAyIC0gYXJyb3dEaW1lbnNpb25zW2xlbmd0aF0gLyAyIC0gMTtcbiAgICBjb25zdCBtaW5QYWRkaW5nID0gbWluKHBhZGRpbmdPYmplY3RbbWluUHJvcF0sIGxhcmdlc3RQb3NzaWJsZVBhZGRpbmcpO1xuICAgIGNvbnN0IG1heFBhZGRpbmcgPSBtaW4ocGFkZGluZ09iamVjdFttYXhQcm9wXSwgbGFyZ2VzdFBvc3NpYmxlUGFkZGluZyk7XG5cbiAgICAvLyBNYWtlIHN1cmUgdGhlIGFycm93IGRvZXNuJ3Qgb3ZlcmZsb3cgdGhlIGZsb2F0aW5nIGVsZW1lbnQgaWYgdGhlIGNlbnRlclxuICAgIC8vIHBvaW50IGlzIG91dHNpZGUgdGhlIGZsb2F0aW5nIGVsZW1lbnQncyBib3VuZHMuXG4gICAgY29uc3QgbWluJDEgPSBtaW5QYWRkaW5nO1xuICAgIGNvbnN0IG1heCA9IGNsaWVudFNpemUgLSBhcnJvd0RpbWVuc2lvbnNbbGVuZ3RoXSAtIG1heFBhZGRpbmc7XG4gICAgY29uc3QgY2VudGVyID0gY2xpZW50U2l6ZSAvIDIgLSBhcnJvd0RpbWVuc2lvbnNbbGVuZ3RoXSAvIDIgKyBjZW50ZXJUb1JlZmVyZW5jZTtcbiAgICBjb25zdCBvZmZzZXQgPSBjbGFtcChtaW4kMSwgY2VudGVyLCBtYXgpO1xuXG4gICAgLy8gSWYgdGhlIHJlZmVyZW5jZSBpcyBzbWFsbCBlbm91Z2ggdGhhdCB0aGUgYXJyb3cncyBwYWRkaW5nIGNhdXNlcyBpdCB0b1xuICAgIC8vIHRvIHBvaW50IHRvIG5vdGhpbmcgZm9yIGFuIGFsaWduZWQgcGxhY2VtZW50LCBhZGp1c3QgdGhlIG9mZnNldCBvZiB0aGVcbiAgICAvLyBmbG9hdGluZyBlbGVtZW50IGl0c2VsZi4gVG8gZW5zdXJlIGBzaGlmdCgpYCBjb250aW51ZXMgdG8gdGFrZSBhY3Rpb24sXG4gICAgLy8gYSBzaW5nbGUgcmVzZXQgaXMgcGVyZm9ybWVkIHdoZW4gdGhpcyBpcyB0cnVlLlxuICAgIGNvbnN0IHNob3VsZEFkZE9mZnNldCA9ICFtaWRkbGV3YXJlRGF0YS5hcnJvdyAmJiBnZXRBbGlnbm1lbnQocGxhY2VtZW50KSAhPSBudWxsICYmIGNlbnRlciAhPT0gb2Zmc2V0ICYmIHJlY3RzLnJlZmVyZW5jZVtsZW5ndGhdIC8gMiAtIChjZW50ZXIgPCBtaW4kMSA/IG1pblBhZGRpbmcgOiBtYXhQYWRkaW5nKSAtIGFycm93RGltZW5zaW9uc1tsZW5ndGhdIC8gMiA8IDA7XG4gICAgY29uc3QgYWxpZ25tZW50T2Zmc2V0ID0gc2hvdWxkQWRkT2Zmc2V0ID8gY2VudGVyIDwgbWluJDEgPyBjZW50ZXIgLSBtaW4kMSA6IGNlbnRlciAtIG1heCA6IDA7XG4gICAgcmV0dXJuIHtcbiAgICAgIFtheGlzXTogY29vcmRzW2F4aXNdICsgYWxpZ25tZW50T2Zmc2V0LFxuICAgICAgZGF0YToge1xuICAgICAgICBbYXhpc106IG9mZnNldCxcbiAgICAgICAgY2VudGVyT2Zmc2V0OiBjZW50ZXIgLSBvZmZzZXQgLSBhbGlnbm1lbnRPZmZzZXQsXG4gICAgICAgIC4uLihzaG91bGRBZGRPZmZzZXQgJiYge1xuICAgICAgICAgIGFsaWdubWVudE9mZnNldFxuICAgICAgICB9KVxuICAgICAgfSxcbiAgICAgIHJlc2V0OiBzaG91bGRBZGRPZmZzZXRcbiAgICB9O1xuICB9XG59KTtcblxuZnVuY3Rpb24gZ2V0UGxhY2VtZW50TGlzdChhbGlnbm1lbnQsIGF1dG9BbGlnbm1lbnQsIGFsbG93ZWRQbGFjZW1lbnRzKSB7XG4gIGNvbnN0IGFsbG93ZWRQbGFjZW1lbnRzU29ydGVkQnlBbGlnbm1lbnQgPSBhbGlnbm1lbnQgPyBbLi4uYWxsb3dlZFBsYWNlbWVudHMuZmlsdGVyKHBsYWNlbWVudCA9PiBnZXRBbGlnbm1lbnQocGxhY2VtZW50KSA9PT0gYWxpZ25tZW50KSwgLi4uYWxsb3dlZFBsYWNlbWVudHMuZmlsdGVyKHBsYWNlbWVudCA9PiBnZXRBbGlnbm1lbnQocGxhY2VtZW50KSAhPT0gYWxpZ25tZW50KV0gOiBhbGxvd2VkUGxhY2VtZW50cy5maWx0ZXIocGxhY2VtZW50ID0+IGdldFNpZGUocGxhY2VtZW50KSA9PT0gcGxhY2VtZW50KTtcbiAgcmV0dXJuIGFsbG93ZWRQbGFjZW1lbnRzU29ydGVkQnlBbGlnbm1lbnQuZmlsdGVyKHBsYWNlbWVudCA9PiB7XG4gICAgaWYgKGFsaWdubWVudCkge1xuICAgICAgcmV0dXJuIGdldEFsaWdubWVudChwbGFjZW1lbnQpID09PSBhbGlnbm1lbnQgfHwgKGF1dG9BbGlnbm1lbnQgPyBnZXRPcHBvc2l0ZUFsaWdubWVudFBsYWNlbWVudChwbGFjZW1lbnQpICE9PSBwbGFjZW1lbnQgOiBmYWxzZSk7XG4gICAgfVxuICAgIHJldHVybiB0cnVlO1xuICB9KTtcbn1cbi8qKlxuICogT3B0aW1pemVzIHRoZSB2aXNpYmlsaXR5IG9mIHRoZSBmbG9hdGluZyBlbGVtZW50IGJ5IGNob29zaW5nIHRoZSBwbGFjZW1lbnRcbiAqIHRoYXQgaGFzIHRoZSBtb3N0IHNwYWNlIGF2YWlsYWJsZSBhdXRvbWF0aWNhbGx5LCB3aXRob3V0IG5lZWRpbmcgdG8gc3BlY2lmeSBhXG4gKiBwcmVmZXJyZWQgcGxhY2VtZW50LiBBbHRlcm5hdGl2ZSB0byBgZmxpcGAuXG4gKiBAc2VlIGh0dHBzOi8vZmxvYXRpbmctdWkuY29tL2RvY3MvYXV0b1BsYWNlbWVudFxuICovXG5jb25zdCBhdXRvUGxhY2VtZW50ID0gZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgaWYgKG9wdGlvbnMgPT09IHZvaWQgMCkge1xuICAgIG9wdGlvbnMgPSB7fTtcbiAgfVxuICByZXR1cm4ge1xuICAgIG5hbWU6ICdhdXRvUGxhY2VtZW50JyxcbiAgICBvcHRpb25zLFxuICAgIGFzeW5jIGZuKHN0YXRlKSB7XG4gICAgICB2YXIgX21pZGRsZXdhcmVEYXRhJGF1dG9QLCBfbWlkZGxld2FyZURhdGEkYXV0b1AyLCBfcGxhY2VtZW50c1RoYXRGaXRPbkU7XG4gICAgICBjb25zdCB7XG4gICAgICAgIHJlY3RzLFxuICAgICAgICBtaWRkbGV3YXJlRGF0YSxcbiAgICAgICAgcGxhY2VtZW50LFxuICAgICAgICBwbGF0Zm9ybSxcbiAgICAgICAgZWxlbWVudHNcbiAgICAgIH0gPSBzdGF0ZTtcbiAgICAgIGNvbnN0IHtcbiAgICAgICAgY3Jvc3NBeGlzID0gZmFsc2UsXG4gICAgICAgIGFsaWdubWVudCxcbiAgICAgICAgYWxsb3dlZFBsYWNlbWVudHMgPSBwbGFjZW1lbnRzLFxuICAgICAgICBhdXRvQWxpZ25tZW50ID0gdHJ1ZSxcbiAgICAgICAgLi4uZGV0ZWN0T3ZlcmZsb3dPcHRpb25zXG4gICAgICB9ID0gZXZhbHVhdGUob3B0aW9ucywgc3RhdGUpO1xuICAgICAgY29uc3QgcGxhY2VtZW50cyQxID0gYWxpZ25tZW50ICE9PSB1bmRlZmluZWQgfHwgYWxsb3dlZFBsYWNlbWVudHMgPT09IHBsYWNlbWVudHMgPyBnZXRQbGFjZW1lbnRMaXN0KGFsaWdubWVudCB8fCBudWxsLCBhdXRvQWxpZ25tZW50LCBhbGxvd2VkUGxhY2VtZW50cykgOiBhbGxvd2VkUGxhY2VtZW50cztcbiAgICAgIGNvbnN0IG92ZXJmbG93ID0gYXdhaXQgZGV0ZWN0T3ZlcmZsb3coc3RhdGUsIGRldGVjdE92ZXJmbG93T3B0aW9ucyk7XG4gICAgICBjb25zdCBjdXJyZW50SW5kZXggPSAoKF9taWRkbGV3YXJlRGF0YSRhdXRvUCA9IG1pZGRsZXdhcmVEYXRhLmF1dG9QbGFjZW1lbnQpID09IG51bGwgPyB2b2lkIDAgOiBfbWlkZGxld2FyZURhdGEkYXV0b1AuaW5kZXgpIHx8IDA7XG4gICAgICBjb25zdCBjdXJyZW50UGxhY2VtZW50ID0gcGxhY2VtZW50cyQxW2N1cnJlbnRJbmRleF07XG4gICAgICBpZiAoY3VycmVudFBsYWNlbWVudCA9PSBudWxsKSB7XG4gICAgICAgIHJldHVybiB7fTtcbiAgICAgIH1cbiAgICAgIGNvbnN0IGFsaWdubWVudFNpZGVzID0gZ2V0QWxpZ25tZW50U2lkZXMoY3VycmVudFBsYWNlbWVudCwgcmVjdHMsIGF3YWl0IChwbGF0Zm9ybS5pc1JUTCA9PSBudWxsID8gdm9pZCAwIDogcGxhdGZvcm0uaXNSVEwoZWxlbWVudHMuZmxvYXRpbmcpKSk7XG5cbiAgICAgIC8vIE1ha2UgYGNvbXB1dGVDb29yZHNgIHN0YXJ0IGZyb20gdGhlIHJpZ2h0IHBsYWNlLlxuICAgICAgaWYgKHBsYWNlbWVudCAhPT0gY3VycmVudFBsYWNlbWVudCkge1xuICAgICAgICByZXR1cm4ge1xuICAgICAgICAgIHJlc2V0OiB7XG4gICAgICAgICAgICBwbGFjZW1lbnQ6IHBsYWNlbWVudHMkMVswXVxuICAgICAgICAgIH1cbiAgICAgICAgfTtcbiAgICAgIH1cbiAgICAgIGNvbnN0IGN1cnJlbnRPdmVyZmxvd3MgPSBbb3ZlcmZsb3dbZ2V0U2lkZShjdXJyZW50UGxhY2VtZW50KV0sIG92ZXJmbG93W2FsaWdubWVudFNpZGVzWzBdXSwgb3ZlcmZsb3dbYWxpZ25tZW50U2lkZXNbMV1dXTtcbiAgICAgIGNvbnN0IGFsbE92ZXJmbG93cyA9IFsuLi4oKChfbWlkZGxld2FyZURhdGEkYXV0b1AyID0gbWlkZGxld2FyZURhdGEuYXV0b1BsYWNlbWVudCkgPT0gbnVsbCA/IHZvaWQgMCA6IF9taWRkbGV3YXJlRGF0YSRhdXRvUDIub3ZlcmZsb3dzKSB8fCBbXSksIHtcbiAgICAgICAgcGxhY2VtZW50OiBjdXJyZW50UGxhY2VtZW50LFxuICAgICAgICBvdmVyZmxvd3M6IGN1cnJlbnRPdmVyZmxvd3NcbiAgICAgIH1dO1xuICAgICAgY29uc3QgbmV4dFBsYWNlbWVudCA9IHBsYWNlbWVudHMkMVtjdXJyZW50SW5kZXggKyAxXTtcblxuICAgICAgLy8gVGhlcmUgYXJlIG1vcmUgcGxhY2VtZW50cyB0byBjaGVjay5cbiAgICAgIGlmIChuZXh0UGxhY2VtZW50KSB7XG4gICAgICAgIHJldHVybiB7XG4gICAgICAgICAgZGF0YToge1xuICAgICAgICAgICAgaW5kZXg6IGN1cnJlbnRJbmRleCArIDEsXG4gICAgICAgICAgICBvdmVyZmxvd3M6IGFsbE92ZXJmbG93c1xuICAgICAgICAgIH0sXG4gICAgICAgICAgcmVzZXQ6IHtcbiAgICAgICAgICAgIHBsYWNlbWVudDogbmV4dFBsYWNlbWVudFxuICAgICAgICAgIH1cbiAgICAgICAgfTtcbiAgICAgIH1cbiAgICAgIGNvbnN0IHBsYWNlbWVudHNTb3J0ZWRCeU1vc3RTcGFjZSA9IGFsbE92ZXJmbG93cy5tYXAoZCA9PiB7XG4gICAgICAgIGNvbnN0IGFsaWdubWVudCA9IGdldEFsaWdubWVudChkLnBsYWNlbWVudCk7XG4gICAgICAgIHJldHVybiBbZC5wbGFjZW1lbnQsIGFsaWdubWVudCAmJiBjcm9zc0F4aXMgP1xuICAgICAgICAvLyBDaGVjayBhbG9uZyB0aGUgbWFpbkF4aXMgYW5kIG1haW4gY3Jvc3NBeGlzIHNpZGUuXG4gICAgICAgIGQub3ZlcmZsb3dzLnNsaWNlKDAsIDIpLnJlZHVjZSgoYWNjLCB2KSA9PiBhY2MgKyB2LCAwKSA6XG4gICAgICAgIC8vIENoZWNrIG9ubHkgdGhlIG1haW5BeGlzLlxuICAgICAgICBkLm92ZXJmbG93c1swXSwgZC5vdmVyZmxvd3NdO1xuICAgICAgfSkuc29ydCgoYSwgYikgPT4gYVsxXSAtIGJbMV0pO1xuICAgICAgY29uc3QgcGxhY2VtZW50c1RoYXRGaXRPbkVhY2hTaWRlID0gcGxhY2VtZW50c1NvcnRlZEJ5TW9zdFNwYWNlLmZpbHRlcihkID0+IGRbMl0uc2xpY2UoMCxcbiAgICAgIC8vIEFsaWduZWQgcGxhY2VtZW50cyBzaG91bGQgbm90IGNoZWNrIHRoZWlyIG9wcG9zaXRlIGNyb3NzQXhpc1xuICAgICAgLy8gc2lkZS5cbiAgICAgIGdldEFsaWdubWVudChkWzBdKSA/IDIgOiAzKS5ldmVyeSh2ID0+IHYgPD0gMCkpO1xuICAgICAgY29uc3QgcmVzZXRQbGFjZW1lbnQgPSAoKF9wbGFjZW1lbnRzVGhhdEZpdE9uRSA9IHBsYWNlbWVudHNUaGF0Rml0T25FYWNoU2lkZVswXSkgPT0gbnVsbCA/IHZvaWQgMCA6IF9wbGFjZW1lbnRzVGhhdEZpdE9uRVswXSkgfHwgcGxhY2VtZW50c1NvcnRlZEJ5TW9zdFNwYWNlWzBdWzBdO1xuICAgICAgaWYgKHJlc2V0UGxhY2VtZW50ICE9PSBwbGFjZW1lbnQpIHtcbiAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICBkYXRhOiB7XG4gICAgICAgICAgICBpbmRleDogY3VycmVudEluZGV4ICsgMSxcbiAgICAgICAgICAgIG92ZXJmbG93czogYWxsT3ZlcmZsb3dzXG4gICAgICAgICAgfSxcbiAgICAgICAgICByZXNldDoge1xuICAgICAgICAgICAgcGxhY2VtZW50OiByZXNldFBsYWNlbWVudFxuICAgICAgICAgIH1cbiAgICAgICAgfTtcbiAgICAgIH1cbiAgICAgIHJldHVybiB7fTtcbiAgICB9XG4gIH07XG59O1xuXG4vKipcbiAqIE9wdGltaXplcyB0aGUgdmlzaWJpbGl0eSBvZiB0aGUgZmxvYXRpbmcgZWxlbWVudCBieSBmbGlwcGluZyB0aGUgYHBsYWNlbWVudGBcbiAqIGluIG9yZGVyIHRvIGtlZXAgaXQgaW4gdmlldyB3aGVuIHRoZSBwcmVmZXJyZWQgcGxhY2VtZW50KHMpIHdpbGwgb3ZlcmZsb3cgdGhlXG4gKiBjbGlwcGluZyBib3VuZGFyeS4gQWx0ZXJuYXRpdmUgdG8gYGF1dG9QbGFjZW1lbnRgLlxuICogQHNlZSBodHRwczovL2Zsb2F0aW5nLXVpLmNvbS9kb2NzL2ZsaXBcbiAqL1xuY29uc3QgZmxpcCA9IGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gIGlmIChvcHRpb25zID09PSB2b2lkIDApIHtcbiAgICBvcHRpb25zID0ge307XG4gIH1cbiAgcmV0dXJuIHtcbiAgICBuYW1lOiAnZmxpcCcsXG4gICAgb3B0aW9ucyxcbiAgICBhc3luYyBmbihzdGF0ZSkge1xuICAgICAgdmFyIF9taWRkbGV3YXJlRGF0YSRhcnJvdywgX21pZGRsZXdhcmVEYXRhJGZsaXA7XG4gICAgICBjb25zdCB7XG4gICAgICAgIHBsYWNlbWVudCxcbiAgICAgICAgbWlkZGxld2FyZURhdGEsXG4gICAgICAgIHJlY3RzLFxuICAgICAgICBpbml0aWFsUGxhY2VtZW50LFxuICAgICAgICBwbGF0Zm9ybSxcbiAgICAgICAgZWxlbWVudHNcbiAgICAgIH0gPSBzdGF0ZTtcbiAgICAgIGNvbnN0IHtcbiAgICAgICAgbWFpbkF4aXM6IGNoZWNrTWFpbkF4aXMgPSB0cnVlLFxuICAgICAgICBjcm9zc0F4aXM6IGNoZWNrQ3Jvc3NBeGlzID0gdHJ1ZSxcbiAgICAgICAgZmFsbGJhY2tQbGFjZW1lbnRzOiBzcGVjaWZpZWRGYWxsYmFja1BsYWNlbWVudHMsXG4gICAgICAgIGZhbGxiYWNrU3RyYXRlZ3kgPSAnYmVzdEZpdCcsXG4gICAgICAgIGZhbGxiYWNrQXhpc1NpZGVEaXJlY3Rpb24gPSAnbm9uZScsXG4gICAgICAgIGZsaXBBbGlnbm1lbnQgPSB0cnVlLFxuICAgICAgICAuLi5kZXRlY3RPdmVyZmxvd09wdGlvbnNcbiAgICAgIH0gPSBldmFsdWF0ZShvcHRpb25zLCBzdGF0ZSk7XG5cbiAgICAgIC8vIElmIGEgcmVzZXQgYnkgdGhlIGFycm93IHdhcyBjYXVzZWQgZHVlIHRvIGFuIGFsaWdubWVudCBvZmZzZXQgYmVpbmdcbiAgICAgIC8vIGFkZGVkLCB3ZSBzaG91bGQgc2tpcCBhbnkgbG9naWMgbm93IHNpbmNlIGBmbGlwKClgIGhhcyBhbHJlYWR5IGRvbmUgaXRzXG4gICAgICAvLyB3b3JrLlxuICAgICAgLy8gaHR0cHM6Ly9naXRodWIuY29tL2Zsb2F0aW5nLXVpL2Zsb2F0aW5nLXVpL2lzc3Vlcy8yNTQ5I2lzc3VlY29tbWVudC0xNzE5NjAxNjQzXG4gICAgICBpZiAoKF9taWRkbGV3YXJlRGF0YSRhcnJvdyA9IG1pZGRsZXdhcmVEYXRhLmFycm93KSAhPSBudWxsICYmIF9taWRkbGV3YXJlRGF0YSRhcnJvdy5hbGlnbm1lbnRPZmZzZXQpIHtcbiAgICAgICAgcmV0dXJuIHt9O1xuICAgICAgfVxuICAgICAgY29uc3Qgc2lkZSA9IGdldFNpZGUocGxhY2VtZW50KTtcbiAgICAgIGNvbnN0IGluaXRpYWxTaWRlQXhpcyA9IGdldFNpZGVBeGlzKGluaXRpYWxQbGFjZW1lbnQpO1xuICAgICAgY29uc3QgaXNCYXNlUGxhY2VtZW50ID0gZ2V0U2lkZShpbml0aWFsUGxhY2VtZW50KSA9PT0gaW5pdGlhbFBsYWNlbWVudDtcbiAgICAgIGNvbnN0IHJ0bCA9IGF3YWl0IChwbGF0Zm9ybS5pc1JUTCA9PSBudWxsID8gdm9pZCAwIDogcGxhdGZvcm0uaXNSVEwoZWxlbWVudHMuZmxvYXRpbmcpKTtcbiAgICAgIGNvbnN0IGZhbGxiYWNrUGxhY2VtZW50cyA9IHNwZWNpZmllZEZhbGxiYWNrUGxhY2VtZW50cyB8fCAoaXNCYXNlUGxhY2VtZW50IHx8ICFmbGlwQWxpZ25tZW50ID8gW2dldE9wcG9zaXRlUGxhY2VtZW50KGluaXRpYWxQbGFjZW1lbnQpXSA6IGdldEV4cGFuZGVkUGxhY2VtZW50cyhpbml0aWFsUGxhY2VtZW50KSk7XG4gICAgICBjb25zdCBoYXNGYWxsYmFja0F4aXNTaWRlRGlyZWN0aW9uID0gZmFsbGJhY2tBeGlzU2lkZURpcmVjdGlvbiAhPT0gJ25vbmUnO1xuICAgICAgaWYgKCFzcGVjaWZpZWRGYWxsYmFja1BsYWNlbWVudHMgJiYgaGFzRmFsbGJhY2tBeGlzU2lkZURpcmVjdGlvbikge1xuICAgICAgICBmYWxsYmFja1BsYWNlbWVudHMucHVzaCguLi5nZXRPcHBvc2l0ZUF4aXNQbGFjZW1lbnRzKGluaXRpYWxQbGFjZW1lbnQsIGZsaXBBbGlnbm1lbnQsIGZhbGxiYWNrQXhpc1NpZGVEaXJlY3Rpb24sIHJ0bCkpO1xuICAgICAgfVxuICAgICAgY29uc3QgcGxhY2VtZW50cyA9IFtpbml0aWFsUGxhY2VtZW50LCAuLi5mYWxsYmFja1BsYWNlbWVudHNdO1xuICAgICAgY29uc3Qgb3ZlcmZsb3cgPSBhd2FpdCBkZXRlY3RPdmVyZmxvdyhzdGF0ZSwgZGV0ZWN0T3ZlcmZsb3dPcHRpb25zKTtcbiAgICAgIGNvbnN0IG92ZXJmbG93cyA9IFtdO1xuICAgICAgbGV0IG92ZXJmbG93c0RhdGEgPSAoKF9taWRkbGV3YXJlRGF0YSRmbGlwID0gbWlkZGxld2FyZURhdGEuZmxpcCkgPT0gbnVsbCA/IHZvaWQgMCA6IF9taWRkbGV3YXJlRGF0YSRmbGlwLm92ZXJmbG93cykgfHwgW107XG4gICAgICBpZiAoY2hlY2tNYWluQXhpcykge1xuICAgICAgICBvdmVyZmxvd3MucHVzaChvdmVyZmxvd1tzaWRlXSk7XG4gICAgICB9XG4gICAgICBpZiAoY2hlY2tDcm9zc0F4aXMpIHtcbiAgICAgICAgY29uc3Qgc2lkZXMgPSBnZXRBbGlnbm1lbnRTaWRlcyhwbGFjZW1lbnQsIHJlY3RzLCBydGwpO1xuICAgICAgICBvdmVyZmxvd3MucHVzaChvdmVyZmxvd1tzaWRlc1swXV0sIG92ZXJmbG93W3NpZGVzWzFdXSk7XG4gICAgICB9XG4gICAgICBvdmVyZmxvd3NEYXRhID0gWy4uLm92ZXJmbG93c0RhdGEsIHtcbiAgICAgICAgcGxhY2VtZW50LFxuICAgICAgICBvdmVyZmxvd3NcbiAgICAgIH1dO1xuXG4gICAgICAvLyBPbmUgb3IgbW9yZSBzaWRlcyBpcyBvdmVyZmxvd2luZy5cbiAgICAgIGlmICghb3ZlcmZsb3dzLmV2ZXJ5KHNpZGUgPT4gc2lkZSA8PSAwKSkge1xuICAgICAgICB2YXIgX21pZGRsZXdhcmVEYXRhJGZsaXAyLCBfb3ZlcmZsb3dzRGF0YSRmaWx0ZXI7XG4gICAgICAgIGNvbnN0IG5leHRJbmRleCA9ICgoKF9taWRkbGV3YXJlRGF0YSRmbGlwMiA9IG1pZGRsZXdhcmVEYXRhLmZsaXApID09IG51bGwgPyB2b2lkIDAgOiBfbWlkZGxld2FyZURhdGEkZmxpcDIuaW5kZXgpIHx8IDApICsgMTtcbiAgICAgICAgY29uc3QgbmV4dFBsYWNlbWVudCA9IHBsYWNlbWVudHNbbmV4dEluZGV4XTtcbiAgICAgICAgaWYgKG5leHRQbGFjZW1lbnQpIHtcbiAgICAgICAgICAvLyBUcnkgbmV4dCBwbGFjZW1lbnQgYW5kIHJlLXJ1biB0aGUgbGlmZWN5Y2xlLlxuICAgICAgICAgIHJldHVybiB7XG4gICAgICAgICAgICBkYXRhOiB7XG4gICAgICAgICAgICAgIGluZGV4OiBuZXh0SW5kZXgsXG4gICAgICAgICAgICAgIG92ZXJmbG93czogb3ZlcmZsb3dzRGF0YVxuICAgICAgICAgICAgfSxcbiAgICAgICAgICAgIHJlc2V0OiB7XG4gICAgICAgICAgICAgIHBsYWNlbWVudDogbmV4dFBsYWNlbWVudFxuICAgICAgICAgICAgfVxuICAgICAgICAgIH07XG4gICAgICAgIH1cblxuICAgICAgICAvLyBGaXJzdCwgZmluZCB0aGUgY2FuZGlkYXRlcyB0aGF0IGZpdCBvbiB0aGUgbWFpbkF4aXMgc2lkZSBvZiBvdmVyZmxvdyxcbiAgICAgICAgLy8gdGhlbiBmaW5kIHRoZSBwbGFjZW1lbnQgdGhhdCBmaXRzIHRoZSBiZXN0IG9uIHRoZSBtYWluIGNyb3NzQXhpcyBzaWRlLlxuICAgICAgICBsZXQgcmVzZXRQbGFjZW1lbnQgPSAoX292ZXJmbG93c0RhdGEkZmlsdGVyID0gb3ZlcmZsb3dzRGF0YS5maWx0ZXIoZCA9PiBkLm92ZXJmbG93c1swXSA8PSAwKS5zb3J0KChhLCBiKSA9PiBhLm92ZXJmbG93c1sxXSAtIGIub3ZlcmZsb3dzWzFdKVswXSkgPT0gbnVsbCA/IHZvaWQgMCA6IF9vdmVyZmxvd3NEYXRhJGZpbHRlci5wbGFjZW1lbnQ7XG5cbiAgICAgICAgLy8gT3RoZXJ3aXNlIGZhbGxiYWNrLlxuICAgICAgICBpZiAoIXJlc2V0UGxhY2VtZW50KSB7XG4gICAgICAgICAgc3dpdGNoIChmYWxsYmFja1N0cmF0ZWd5KSB7XG4gICAgICAgICAgICBjYXNlICdiZXN0Rml0JzpcbiAgICAgICAgICAgICAge1xuICAgICAgICAgICAgICAgIHZhciBfb3ZlcmZsb3dzRGF0YSRmaWx0ZXIyO1xuICAgICAgICAgICAgICAgIGNvbnN0IHBsYWNlbWVudCA9IChfb3ZlcmZsb3dzRGF0YSRmaWx0ZXIyID0gb3ZlcmZsb3dzRGF0YS5maWx0ZXIoZCA9PiB7XG4gICAgICAgICAgICAgICAgICBpZiAoaGFzRmFsbGJhY2tBeGlzU2lkZURpcmVjdGlvbikge1xuICAgICAgICAgICAgICAgICAgICBjb25zdCBjdXJyZW50U2lkZUF4aXMgPSBnZXRTaWRlQXhpcyhkLnBsYWNlbWVudCk7XG4gICAgICAgICAgICAgICAgICAgIHJldHVybiBjdXJyZW50U2lkZUF4aXMgPT09IGluaXRpYWxTaWRlQXhpcyB8fFxuICAgICAgICAgICAgICAgICAgICAvLyBDcmVhdGUgYSBiaWFzIHRvIHRoZSBgeWAgc2lkZSBheGlzIGR1ZSB0byBob3Jpem9udGFsXG4gICAgICAgICAgICAgICAgICAgIC8vIHJlYWRpbmcgZGlyZWN0aW9ucyBmYXZvcmluZyBncmVhdGVyIHdpZHRoLlxuICAgICAgICAgICAgICAgICAgICBjdXJyZW50U2lkZUF4aXMgPT09ICd5JztcbiAgICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICAgIHJldHVybiB0cnVlO1xuICAgICAgICAgICAgICAgIH0pLm1hcChkID0+IFtkLnBsYWNlbWVudCwgZC5vdmVyZmxvd3MuZmlsdGVyKG92ZXJmbG93ID0+IG92ZXJmbG93ID4gMCkucmVkdWNlKChhY2MsIG92ZXJmbG93KSA9PiBhY2MgKyBvdmVyZmxvdywgMCldKS5zb3J0KChhLCBiKSA9PiBhWzFdIC0gYlsxXSlbMF0pID09IG51bGwgPyB2b2lkIDAgOiBfb3ZlcmZsb3dzRGF0YSRmaWx0ZXIyWzBdO1xuICAgICAgICAgICAgICAgIGlmIChwbGFjZW1lbnQpIHtcbiAgICAgICAgICAgICAgICAgIHJlc2V0UGxhY2VtZW50ID0gcGxhY2VtZW50O1xuICAgICAgICAgICAgICAgIH1cbiAgICAgICAgICAgICAgICBicmVhaztcbiAgICAgICAgICAgICAgfVxuICAgICAgICAgICAgY2FzZSAnaW5pdGlhbFBsYWNlbWVudCc6XG4gICAgICAgICAgICAgIHJlc2V0UGxhY2VtZW50ID0gaW5pdGlhbFBsYWNlbWVudDtcbiAgICAgICAgICAgICAgYnJlYWs7XG4gICAgICAgICAgfVxuICAgICAgICB9XG4gICAgICAgIGlmIChwbGFjZW1lbnQgIT09IHJlc2V0UGxhY2VtZW50KSB7XG4gICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgIHJlc2V0OiB7XG4gICAgICAgICAgICAgIHBsYWNlbWVudDogcmVzZXRQbGFjZW1lbnRcbiAgICAgICAgICAgIH1cbiAgICAgICAgICB9O1xuICAgICAgICB9XG4gICAgICB9XG4gICAgICByZXR1cm4ge307XG4gICAgfVxuICB9O1xufTtcblxuZnVuY3Rpb24gZ2V0U2lkZU9mZnNldHMob3ZlcmZsb3csIHJlY3QpIHtcbiAgcmV0dXJuIHtcbiAgICB0b3A6IG92ZXJmbG93LnRvcCAtIHJlY3QuaGVpZ2h0LFxuICAgIHJpZ2h0OiBvdmVyZmxvdy5yaWdodCAtIHJlY3Qud2lkdGgsXG4gICAgYm90dG9tOiBvdmVyZmxvdy5ib3R0b20gLSByZWN0LmhlaWdodCxcbiAgICBsZWZ0OiBvdmVyZmxvdy5sZWZ0IC0gcmVjdC53aWR0aFxuICB9O1xufVxuZnVuY3Rpb24gaXNBbnlTaWRlRnVsbHlDbGlwcGVkKG92ZXJmbG93KSB7XG4gIHJldHVybiBzaWRlcy5zb21lKHNpZGUgPT4gb3ZlcmZsb3dbc2lkZV0gPj0gMCk7XG59XG4vKipcbiAqIFByb3ZpZGVzIGRhdGEgdG8gaGlkZSB0aGUgZmxvYXRpbmcgZWxlbWVudCBpbiBhcHBsaWNhYmxlIHNpdHVhdGlvbnMsIHN1Y2ggYXNcbiAqIHdoZW4gaXQgaXMgbm90IGluIHRoZSBzYW1lIGNsaXBwaW5nIGNvbnRleHQgYXMgdGhlIHJlZmVyZW5jZSBlbGVtZW50LlxuICogQHNlZSBodHRwczovL2Zsb2F0aW5nLXVpLmNvbS9kb2NzL2hpZGVcbiAqL1xuY29uc3QgaGlkZSA9IGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gIGlmIChvcHRpb25zID09PSB2b2lkIDApIHtcbiAgICBvcHRpb25zID0ge307XG4gIH1cbiAgcmV0dXJuIHtcbiAgICBuYW1lOiAnaGlkZScsXG4gICAgb3B0aW9ucyxcbiAgICBhc3luYyBmbihzdGF0ZSkge1xuICAgICAgY29uc3Qge1xuICAgICAgICByZWN0c1xuICAgICAgfSA9IHN0YXRlO1xuICAgICAgY29uc3Qge1xuICAgICAgICBzdHJhdGVneSA9ICdyZWZlcmVuY2VIaWRkZW4nLFxuICAgICAgICAuLi5kZXRlY3RPdmVyZmxvd09wdGlvbnNcbiAgICAgIH0gPSBldmFsdWF0ZShvcHRpb25zLCBzdGF0ZSk7XG4gICAgICBzd2l0Y2ggKHN0cmF0ZWd5KSB7XG4gICAgICAgIGNhc2UgJ3JlZmVyZW5jZUhpZGRlbic6XG4gICAgICAgICAge1xuICAgICAgICAgICAgY29uc3Qgb3ZlcmZsb3cgPSBhd2FpdCBkZXRlY3RPdmVyZmxvdyhzdGF0ZSwge1xuICAgICAgICAgICAgICAuLi5kZXRlY3RPdmVyZmxvd09wdGlvbnMsXG4gICAgICAgICAgICAgIGVsZW1lbnRDb250ZXh0OiAncmVmZXJlbmNlJ1xuICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICBjb25zdCBvZmZzZXRzID0gZ2V0U2lkZU9mZnNldHMob3ZlcmZsb3csIHJlY3RzLnJlZmVyZW5jZSk7XG4gICAgICAgICAgICByZXR1cm4ge1xuICAgICAgICAgICAgICBkYXRhOiB7XG4gICAgICAgICAgICAgICAgcmVmZXJlbmNlSGlkZGVuT2Zmc2V0czogb2Zmc2V0cyxcbiAgICAgICAgICAgICAgICByZWZlcmVuY2VIaWRkZW46IGlzQW55U2lkZUZ1bGx5Q2xpcHBlZChvZmZzZXRzKVxuICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9O1xuICAgICAgICAgIH1cbiAgICAgICAgY2FzZSAnZXNjYXBlZCc6XG4gICAgICAgICAge1xuICAgICAgICAgICAgY29uc3Qgb3ZlcmZsb3cgPSBhd2FpdCBkZXRlY3RPdmVyZmxvdyhzdGF0ZSwge1xuICAgICAgICAgICAgICAuLi5kZXRlY3RPdmVyZmxvd09wdGlvbnMsXG4gICAgICAgICAgICAgIGFsdEJvdW5kYXJ5OiB0cnVlXG4gICAgICAgICAgICB9KTtcbiAgICAgICAgICAgIGNvbnN0IG9mZnNldHMgPSBnZXRTaWRlT2Zmc2V0cyhvdmVyZmxvdywgcmVjdHMuZmxvYXRpbmcpO1xuICAgICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgICAgZGF0YToge1xuICAgICAgICAgICAgICAgIGVzY2FwZWRPZmZzZXRzOiBvZmZzZXRzLFxuICAgICAgICAgICAgICAgIGVzY2FwZWQ6IGlzQW55U2lkZUZ1bGx5Q2xpcHBlZChvZmZzZXRzKVxuICAgICAgICAgICAgICB9XG4gICAgICAgICAgICB9O1xuICAgICAgICAgIH1cbiAgICAgICAgZGVmYXVsdDpcbiAgICAgICAgICB7XG4gICAgICAgICAgICByZXR1cm4ge307XG4gICAgICAgICAgfVxuICAgICAgfVxuICAgIH1cbiAgfTtcbn07XG5cbmZ1bmN0aW9uIGdldEJvdW5kaW5nUmVjdChyZWN0cykge1xuICBjb25zdCBtaW5YID0gbWluKC4uLnJlY3RzLm1hcChyZWN0ID0+IHJlY3QubGVmdCkpO1xuICBjb25zdCBtaW5ZID0gbWluKC4uLnJlY3RzLm1hcChyZWN0ID0+IHJlY3QudG9wKSk7XG4gIGNvbnN0IG1heFggPSBtYXgoLi4ucmVjdHMubWFwKHJlY3QgPT4gcmVjdC5yaWdodCkpO1xuICBjb25zdCBtYXhZID0gbWF4KC4uLnJlY3RzLm1hcChyZWN0ID0+IHJlY3QuYm90dG9tKSk7XG4gIHJldHVybiB7XG4gICAgeDogbWluWCxcbiAgICB5OiBtaW5ZLFxuICAgIHdpZHRoOiBtYXhYIC0gbWluWCxcbiAgICBoZWlnaHQ6IG1heFkgLSBtaW5ZXG4gIH07XG59XG5mdW5jdGlvbiBnZXRSZWN0c0J5TGluZShyZWN0cykge1xuICBjb25zdCBzb3J0ZWRSZWN0cyA9IHJlY3RzLnNsaWNlKCkuc29ydCgoYSwgYikgPT4gYS55IC0gYi55KTtcbiAgY29uc3QgZ3JvdXBzID0gW107XG4gIGxldCBwcmV2UmVjdCA9IG51bGw7XG4gIGZvciAobGV0IGkgPSAwOyBpIDwgc29ydGVkUmVjdHMubGVuZ3RoOyBpKyspIHtcbiAgICBjb25zdCByZWN0ID0gc29ydGVkUmVjdHNbaV07XG4gICAgaWYgKCFwcmV2UmVjdCB8fCByZWN0LnkgLSBwcmV2UmVjdC55ID4gcHJldlJlY3QuaGVpZ2h0IC8gMikge1xuICAgICAgZ3JvdXBzLnB1c2goW3JlY3RdKTtcbiAgICB9IGVsc2Uge1xuICAgICAgZ3JvdXBzW2dyb3Vwcy5sZW5ndGggLSAxXS5wdXNoKHJlY3QpO1xuICAgIH1cbiAgICBwcmV2UmVjdCA9IHJlY3Q7XG4gIH1cbiAgcmV0dXJuIGdyb3Vwcy5tYXAocmVjdCA9PiByZWN0VG9DbGllbnRSZWN0KGdldEJvdW5kaW5nUmVjdChyZWN0KSkpO1xufVxuLyoqXG4gKiBQcm92aWRlcyBpbXByb3ZlZCBwb3NpdGlvbmluZyBmb3IgaW5saW5lIHJlZmVyZW5jZSBlbGVtZW50cyB0aGF0IGNhbiBzcGFuXG4gKiBvdmVyIG11bHRpcGxlIGxpbmVzLCBzdWNoIGFzIGh5cGVybGlua3Mgb3IgcmFuZ2Ugc2VsZWN0aW9ucy5cbiAqIEBzZWUgaHR0cHM6Ly9mbG9hdGluZy11aS5jb20vZG9jcy9pbmxpbmVcbiAqL1xuY29uc3QgaW5saW5lID0gZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgaWYgKG9wdGlvbnMgPT09IHZvaWQgMCkge1xuICAgIG9wdGlvbnMgPSB7fTtcbiAgfVxuICByZXR1cm4ge1xuICAgIG5hbWU6ICdpbmxpbmUnLFxuICAgIG9wdGlvbnMsXG4gICAgYXN5bmMgZm4oc3RhdGUpIHtcbiAgICAgIGNvbnN0IHtcbiAgICAgICAgcGxhY2VtZW50LFxuICAgICAgICBlbGVtZW50cyxcbiAgICAgICAgcmVjdHMsXG4gICAgICAgIHBsYXRmb3JtLFxuICAgICAgICBzdHJhdGVneVxuICAgICAgfSA9IHN0YXRlO1xuICAgICAgLy8gQSBNb3VzZUV2ZW50J3MgY2xpZW50e1gsWX0gY29vcmRzIGNhbiBiZSB1cCB0byAyIHBpeGVscyBvZmYgYVxuICAgICAgLy8gQ2xpZW50UmVjdCdzIGJvdW5kcywgZGVzcGl0ZSB0aGUgZXZlbnQgbGlzdGVuZXIgYmVpbmcgdHJpZ2dlcmVkLiBBXG4gICAgICAvLyBwYWRkaW5nIG9mIDIgc2VlbXMgdG8gaGFuZGxlIHRoaXMgaXNzdWUuXG4gICAgICBjb25zdCB7XG4gICAgICAgIHBhZGRpbmcgPSAyLFxuICAgICAgICB4LFxuICAgICAgICB5XG4gICAgICB9ID0gZXZhbHVhdGUob3B0aW9ucywgc3RhdGUpO1xuICAgICAgY29uc3QgbmF0aXZlQ2xpZW50UmVjdHMgPSBBcnJheS5mcm9tKChhd2FpdCAocGxhdGZvcm0uZ2V0Q2xpZW50UmVjdHMgPT0gbnVsbCA/IHZvaWQgMCA6IHBsYXRmb3JtLmdldENsaWVudFJlY3RzKGVsZW1lbnRzLnJlZmVyZW5jZSkpKSB8fCBbXSk7XG4gICAgICBjb25zdCBjbGllbnRSZWN0cyA9IGdldFJlY3RzQnlMaW5lKG5hdGl2ZUNsaWVudFJlY3RzKTtcbiAgICAgIGNvbnN0IGZhbGxiYWNrID0gcmVjdFRvQ2xpZW50UmVjdChnZXRCb3VuZGluZ1JlY3QobmF0aXZlQ2xpZW50UmVjdHMpKTtcbiAgICAgIGNvbnN0IHBhZGRpbmdPYmplY3QgPSBnZXRQYWRkaW5nT2JqZWN0KHBhZGRpbmcpO1xuICAgICAgZnVuY3Rpb24gZ2V0Qm91bmRpbmdDbGllbnRSZWN0KCkge1xuICAgICAgICAvLyBUaGVyZSBhcmUgdHdvIHJlY3RzIGFuZCB0aGV5IGFyZSBkaXNqb2luZWQuXG4gICAgICAgIGlmIChjbGllbnRSZWN0cy5sZW5ndGggPT09IDIgJiYgY2xpZW50UmVjdHNbMF0ubGVmdCA+IGNsaWVudFJlY3RzWzFdLnJpZ2h0ICYmIHggIT0gbnVsbCAmJiB5ICE9IG51bGwpIHtcbiAgICAgICAgICAvLyBGaW5kIHRoZSBmaXJzdCByZWN0IGluIHdoaWNoIHRoZSBwb2ludCBpcyBmdWxseSBpbnNpZGUuXG4gICAgICAgICAgcmV0dXJuIGNsaWVudFJlY3RzLmZpbmQocmVjdCA9PiB4ID4gcmVjdC5sZWZ0IC0gcGFkZGluZ09iamVjdC5sZWZ0ICYmIHggPCByZWN0LnJpZ2h0ICsgcGFkZGluZ09iamVjdC5yaWdodCAmJiB5ID4gcmVjdC50b3AgLSBwYWRkaW5nT2JqZWN0LnRvcCAmJiB5IDwgcmVjdC5ib3R0b20gKyBwYWRkaW5nT2JqZWN0LmJvdHRvbSkgfHwgZmFsbGJhY2s7XG4gICAgICAgIH1cblxuICAgICAgICAvLyBUaGVyZSBhcmUgMiBvciBtb3JlIGNvbm5lY3RlZCByZWN0cy5cbiAgICAgICAgaWYgKGNsaWVudFJlY3RzLmxlbmd0aCA+PSAyKSB7XG4gICAgICAgICAgaWYgKGdldFNpZGVBeGlzKHBsYWNlbWVudCkgPT09ICd5Jykge1xuICAgICAgICAgICAgY29uc3QgZmlyc3RSZWN0ID0gY2xpZW50UmVjdHNbMF07XG4gICAgICAgICAgICBjb25zdCBsYXN0UmVjdCA9IGNsaWVudFJlY3RzW2NsaWVudFJlY3RzLmxlbmd0aCAtIDFdO1xuICAgICAgICAgICAgY29uc3QgaXNUb3AgPSBnZXRTaWRlKHBsYWNlbWVudCkgPT09ICd0b3AnO1xuICAgICAgICAgICAgY29uc3QgdG9wID0gZmlyc3RSZWN0LnRvcDtcbiAgICAgICAgICAgIGNvbnN0IGJvdHRvbSA9IGxhc3RSZWN0LmJvdHRvbTtcbiAgICAgICAgICAgIGNvbnN0IGxlZnQgPSBpc1RvcCA/IGZpcnN0UmVjdC5sZWZ0IDogbGFzdFJlY3QubGVmdDtcbiAgICAgICAgICAgIGNvbnN0IHJpZ2h0ID0gaXNUb3AgPyBmaXJzdFJlY3QucmlnaHQgOiBsYXN0UmVjdC5yaWdodDtcbiAgICAgICAgICAgIGNvbnN0IHdpZHRoID0gcmlnaHQgLSBsZWZ0O1xuICAgICAgICAgICAgY29uc3QgaGVpZ2h0ID0gYm90dG9tIC0gdG9wO1xuICAgICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgICAgdG9wLFxuICAgICAgICAgICAgICBib3R0b20sXG4gICAgICAgICAgICAgIGxlZnQsXG4gICAgICAgICAgICAgIHJpZ2h0LFxuICAgICAgICAgICAgICB3aWR0aCxcbiAgICAgICAgICAgICAgaGVpZ2h0LFxuICAgICAgICAgICAgICB4OiBsZWZ0LFxuICAgICAgICAgICAgICB5OiB0b3BcbiAgICAgICAgICAgIH07XG4gICAgICAgICAgfVxuICAgICAgICAgIGNvbnN0IGlzTGVmdFNpZGUgPSBnZXRTaWRlKHBsYWNlbWVudCkgPT09ICdsZWZ0JztcbiAgICAgICAgICBjb25zdCBtYXhSaWdodCA9IG1heCguLi5jbGllbnRSZWN0cy5tYXAocmVjdCA9PiByZWN0LnJpZ2h0KSk7XG4gICAgICAgICAgY29uc3QgbWluTGVmdCA9IG1pbiguLi5jbGllbnRSZWN0cy5tYXAocmVjdCA9PiByZWN0LmxlZnQpKTtcbiAgICAgICAgICBjb25zdCBtZWFzdXJlUmVjdHMgPSBjbGllbnRSZWN0cy5maWx0ZXIocmVjdCA9PiBpc0xlZnRTaWRlID8gcmVjdC5sZWZ0ID09PSBtaW5MZWZ0IDogcmVjdC5yaWdodCA9PT0gbWF4UmlnaHQpO1xuICAgICAgICAgIGNvbnN0IHRvcCA9IG1lYXN1cmVSZWN0c1swXS50b3A7XG4gICAgICAgICAgY29uc3QgYm90dG9tID0gbWVhc3VyZVJlY3RzW21lYXN1cmVSZWN0cy5sZW5ndGggLSAxXS5ib3R0b207XG4gICAgICAgICAgY29uc3QgbGVmdCA9IG1pbkxlZnQ7XG4gICAgICAgICAgY29uc3QgcmlnaHQgPSBtYXhSaWdodDtcbiAgICAgICAgICBjb25zdCB3aWR0aCA9IHJpZ2h0IC0gbGVmdDtcbiAgICAgICAgICBjb25zdCBoZWlnaHQgPSBib3R0b20gLSB0b3A7XG4gICAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAgIHRvcCxcbiAgICAgICAgICAgIGJvdHRvbSxcbiAgICAgICAgICAgIGxlZnQsXG4gICAgICAgICAgICByaWdodCxcbiAgICAgICAgICAgIHdpZHRoLFxuICAgICAgICAgICAgaGVpZ2h0LFxuICAgICAgICAgICAgeDogbGVmdCxcbiAgICAgICAgICAgIHk6IHRvcFxuICAgICAgICAgIH07XG4gICAgICAgIH1cbiAgICAgICAgcmV0dXJuIGZhbGxiYWNrO1xuICAgICAgfVxuICAgICAgY29uc3QgcmVzZXRSZWN0cyA9IGF3YWl0IHBsYXRmb3JtLmdldEVsZW1lbnRSZWN0cyh7XG4gICAgICAgIHJlZmVyZW5jZToge1xuICAgICAgICAgIGdldEJvdW5kaW5nQ2xpZW50UmVjdFxuICAgICAgICB9LFxuICAgICAgICBmbG9hdGluZzogZWxlbWVudHMuZmxvYXRpbmcsXG4gICAgICAgIHN0cmF0ZWd5XG4gICAgICB9KTtcbiAgICAgIGlmIChyZWN0cy5yZWZlcmVuY2UueCAhPT0gcmVzZXRSZWN0cy5yZWZlcmVuY2UueCB8fCByZWN0cy5yZWZlcmVuY2UueSAhPT0gcmVzZXRSZWN0cy5yZWZlcmVuY2UueSB8fCByZWN0cy5yZWZlcmVuY2Uud2lkdGggIT09IHJlc2V0UmVjdHMucmVmZXJlbmNlLndpZHRoIHx8IHJlY3RzLnJlZmVyZW5jZS5oZWlnaHQgIT09IHJlc2V0UmVjdHMucmVmZXJlbmNlLmhlaWdodCkge1xuICAgICAgICByZXR1cm4ge1xuICAgICAgICAgIHJlc2V0OiB7XG4gICAgICAgICAgICByZWN0czogcmVzZXRSZWN0c1xuICAgICAgICAgIH1cbiAgICAgICAgfTtcbiAgICAgIH1cbiAgICAgIHJldHVybiB7fTtcbiAgICB9XG4gIH07XG59O1xuXG4vLyBGb3IgdHlwZSBiYWNrd2FyZHMtY29tcGF0aWJpbGl0eSwgdGhlIGBPZmZzZXRPcHRpb25zYCB0eXBlIHdhcyBhbHNvXG4vLyBEZXJpdmFibGUuXG5cbmFzeW5jIGZ1bmN0aW9uIGNvbnZlcnRWYWx1ZVRvQ29vcmRzKHN0YXRlLCBvcHRpb25zKSB7XG4gIGNvbnN0IHtcbiAgICBwbGFjZW1lbnQsXG4gICAgcGxhdGZvcm0sXG4gICAgZWxlbWVudHNcbiAgfSA9IHN0YXRlO1xuICBjb25zdCBydGwgPSBhd2FpdCAocGxhdGZvcm0uaXNSVEwgPT0gbnVsbCA/IHZvaWQgMCA6IHBsYXRmb3JtLmlzUlRMKGVsZW1lbnRzLmZsb2F0aW5nKSk7XG4gIGNvbnN0IHNpZGUgPSBnZXRTaWRlKHBsYWNlbWVudCk7XG4gIGNvbnN0IGFsaWdubWVudCA9IGdldEFsaWdubWVudChwbGFjZW1lbnQpO1xuICBjb25zdCBpc1ZlcnRpY2FsID0gZ2V0U2lkZUF4aXMocGxhY2VtZW50KSA9PT0gJ3knO1xuICBjb25zdCBtYWluQXhpc011bHRpID0gWydsZWZ0JywgJ3RvcCddLmluY2x1ZGVzKHNpZGUpID8gLTEgOiAxO1xuICBjb25zdCBjcm9zc0F4aXNNdWx0aSA9IHJ0bCAmJiBpc1ZlcnRpY2FsID8gLTEgOiAxO1xuICBjb25zdCByYXdWYWx1ZSA9IGV2YWx1YXRlKG9wdGlvbnMsIHN0YXRlKTtcblxuICAvLyBlc2xpbnQtZGlzYWJsZS1uZXh0LWxpbmUgcHJlZmVyLWNvbnN0XG4gIGxldCB7XG4gICAgbWFpbkF4aXMsXG4gICAgY3Jvc3NBeGlzLFxuICAgIGFsaWdubWVudEF4aXNcbiAgfSA9IHR5cGVvZiByYXdWYWx1ZSA9PT0gJ251bWJlcicgPyB7XG4gICAgbWFpbkF4aXM6IHJhd1ZhbHVlLFxuICAgIGNyb3NzQXhpczogMCxcbiAgICBhbGlnbm1lbnRBeGlzOiBudWxsXG4gIH0gOiB7XG4gICAgbWFpbkF4aXM6IHJhd1ZhbHVlLm1haW5BeGlzIHx8IDAsXG4gICAgY3Jvc3NBeGlzOiByYXdWYWx1ZS5jcm9zc0F4aXMgfHwgMCxcbiAgICBhbGlnbm1lbnRBeGlzOiByYXdWYWx1ZS5hbGlnbm1lbnRBeGlzXG4gIH07XG4gIGlmIChhbGlnbm1lbnQgJiYgdHlwZW9mIGFsaWdubWVudEF4aXMgPT09ICdudW1iZXInKSB7XG4gICAgY3Jvc3NBeGlzID0gYWxpZ25tZW50ID09PSAnZW5kJyA/IGFsaWdubWVudEF4aXMgKiAtMSA6IGFsaWdubWVudEF4aXM7XG4gIH1cbiAgcmV0dXJuIGlzVmVydGljYWwgPyB7XG4gICAgeDogY3Jvc3NBeGlzICogY3Jvc3NBeGlzTXVsdGksXG4gICAgeTogbWFpbkF4aXMgKiBtYWluQXhpc011bHRpXG4gIH0gOiB7XG4gICAgeDogbWFpbkF4aXMgKiBtYWluQXhpc011bHRpLFxuICAgIHk6IGNyb3NzQXhpcyAqIGNyb3NzQXhpc011bHRpXG4gIH07XG59XG5cbi8qKlxuICogTW9kaWZpZXMgdGhlIHBsYWNlbWVudCBieSB0cmFuc2xhdGluZyB0aGUgZmxvYXRpbmcgZWxlbWVudCBhbG9uZyB0aGVcbiAqIHNwZWNpZmllZCBheGVzLlxuICogQSBudW1iZXIgKHNob3J0aGFuZCBmb3IgYG1haW5BeGlzYCBvciBkaXN0YW5jZSksIG9yIGFuIGF4ZXMgY29uZmlndXJhdGlvblxuICogb2JqZWN0IG1heSBiZSBwYXNzZWQuXG4gKiBAc2VlIGh0dHBzOi8vZmxvYXRpbmctdWkuY29tL2RvY3Mvb2Zmc2V0XG4gKi9cbmNvbnN0IG9mZnNldCA9IGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gIGlmIChvcHRpb25zID09PSB2b2lkIDApIHtcbiAgICBvcHRpb25zID0gMDtcbiAgfVxuICByZXR1cm4ge1xuICAgIG5hbWU6ICdvZmZzZXQnLFxuICAgIG9wdGlvbnMsXG4gICAgYXN5bmMgZm4oc3RhdGUpIHtcbiAgICAgIHZhciBfbWlkZGxld2FyZURhdGEkb2Zmc2UsIF9taWRkbGV3YXJlRGF0YSRhcnJvdztcbiAgICAgIGNvbnN0IHtcbiAgICAgICAgeCxcbiAgICAgICAgeSxcbiAgICAgICAgcGxhY2VtZW50LFxuICAgICAgICBtaWRkbGV3YXJlRGF0YVxuICAgICAgfSA9IHN0YXRlO1xuICAgICAgY29uc3QgZGlmZkNvb3JkcyA9IGF3YWl0IGNvbnZlcnRWYWx1ZVRvQ29vcmRzKHN0YXRlLCBvcHRpb25zKTtcblxuICAgICAgLy8gSWYgdGhlIHBsYWNlbWVudCBpcyB0aGUgc2FtZSBhbmQgdGhlIGFycm93IGNhdXNlZCBhbiBhbGlnbm1lbnQgb2Zmc2V0XG4gICAgICAvLyB0aGVuIHdlIGRvbid0IG5lZWQgdG8gY2hhbmdlIHRoZSBwb3NpdGlvbmluZyBjb29yZGluYXRlcy5cbiAgICAgIGlmIChwbGFjZW1lbnQgPT09ICgoX21pZGRsZXdhcmVEYXRhJG9mZnNlID0gbWlkZGxld2FyZURhdGEub2Zmc2V0KSA9PSBudWxsID8gdm9pZCAwIDogX21pZGRsZXdhcmVEYXRhJG9mZnNlLnBsYWNlbWVudCkgJiYgKF9taWRkbGV3YXJlRGF0YSRhcnJvdyA9IG1pZGRsZXdhcmVEYXRhLmFycm93KSAhPSBudWxsICYmIF9taWRkbGV3YXJlRGF0YSRhcnJvdy5hbGlnbm1lbnRPZmZzZXQpIHtcbiAgICAgICAgcmV0dXJuIHt9O1xuICAgICAgfVxuICAgICAgcmV0dXJuIHtcbiAgICAgICAgeDogeCArIGRpZmZDb29yZHMueCxcbiAgICAgICAgeTogeSArIGRpZmZDb29yZHMueSxcbiAgICAgICAgZGF0YToge1xuICAgICAgICAgIC4uLmRpZmZDb29yZHMsXG4gICAgICAgICAgcGxhY2VtZW50XG4gICAgICAgIH1cbiAgICAgIH07XG4gICAgfVxuICB9O1xufTtcblxuLyoqXG4gKiBPcHRpbWl6ZXMgdGhlIHZpc2liaWxpdHkgb2YgdGhlIGZsb2F0aW5nIGVsZW1lbnQgYnkgc2hpZnRpbmcgaXQgaW4gb3JkZXIgdG9cbiAqIGtlZXAgaXQgaW4gdmlldyB3aGVuIGl0IHdpbGwgb3ZlcmZsb3cgdGhlIGNsaXBwaW5nIGJvdW5kYXJ5LlxuICogQHNlZSBodHRwczovL2Zsb2F0aW5nLXVpLmNvbS9kb2NzL3NoaWZ0XG4gKi9cbmNvbnN0IHNoaWZ0ID0gZnVuY3Rpb24gKG9wdGlvbnMpIHtcbiAgaWYgKG9wdGlvbnMgPT09IHZvaWQgMCkge1xuICAgIG9wdGlvbnMgPSB7fTtcbiAgfVxuICByZXR1cm4ge1xuICAgIG5hbWU6ICdzaGlmdCcsXG4gICAgb3B0aW9ucyxcbiAgICBhc3luYyBmbihzdGF0ZSkge1xuICAgICAgY29uc3Qge1xuICAgICAgICB4LFxuICAgICAgICB5LFxuICAgICAgICBwbGFjZW1lbnRcbiAgICAgIH0gPSBzdGF0ZTtcbiAgICAgIGNvbnN0IHtcbiAgICAgICAgbWFpbkF4aXM6IGNoZWNrTWFpbkF4aXMgPSB0cnVlLFxuICAgICAgICBjcm9zc0F4aXM6IGNoZWNrQ3Jvc3NBeGlzID0gZmFsc2UsXG4gICAgICAgIGxpbWl0ZXIgPSB7XG4gICAgICAgICAgZm46IF9yZWYgPT4ge1xuICAgICAgICAgICAgbGV0IHtcbiAgICAgICAgICAgICAgeCxcbiAgICAgICAgICAgICAgeVxuICAgICAgICAgICAgfSA9IF9yZWY7XG4gICAgICAgICAgICByZXR1cm4ge1xuICAgICAgICAgICAgICB4LFxuICAgICAgICAgICAgICB5XG4gICAgICAgICAgICB9O1xuICAgICAgICAgIH1cbiAgICAgICAgfSxcbiAgICAgICAgLi4uZGV0ZWN0T3ZlcmZsb3dPcHRpb25zXG4gICAgICB9ID0gZXZhbHVhdGUob3B0aW9ucywgc3RhdGUpO1xuICAgICAgY29uc3QgY29vcmRzID0ge1xuICAgICAgICB4LFxuICAgICAgICB5XG4gICAgICB9O1xuICAgICAgY29uc3Qgb3ZlcmZsb3cgPSBhd2FpdCBkZXRlY3RPdmVyZmxvdyhzdGF0ZSwgZGV0ZWN0T3ZlcmZsb3dPcHRpb25zKTtcbiAgICAgIGNvbnN0IGNyb3NzQXhpcyA9IGdldFNpZGVBeGlzKGdldFNpZGUocGxhY2VtZW50KSk7XG4gICAgICBjb25zdCBtYWluQXhpcyA9IGdldE9wcG9zaXRlQXhpcyhjcm9zc0F4aXMpO1xuICAgICAgbGV0IG1haW5BeGlzQ29vcmQgPSBjb29yZHNbbWFpbkF4aXNdO1xuICAgICAgbGV0IGNyb3NzQXhpc0Nvb3JkID0gY29vcmRzW2Nyb3NzQXhpc107XG4gICAgICBpZiAoY2hlY2tNYWluQXhpcykge1xuICAgICAgICBjb25zdCBtaW5TaWRlID0gbWFpbkF4aXMgPT09ICd5JyA/ICd0b3AnIDogJ2xlZnQnO1xuICAgICAgICBjb25zdCBtYXhTaWRlID0gbWFpbkF4aXMgPT09ICd5JyA/ICdib3R0b20nIDogJ3JpZ2h0JztcbiAgICAgICAgY29uc3QgbWluID0gbWFpbkF4aXNDb29yZCArIG92ZXJmbG93W21pblNpZGVdO1xuICAgICAgICBjb25zdCBtYXggPSBtYWluQXhpc0Nvb3JkIC0gb3ZlcmZsb3dbbWF4U2lkZV07XG4gICAgICAgIG1haW5BeGlzQ29vcmQgPSBjbGFtcChtaW4sIG1haW5BeGlzQ29vcmQsIG1heCk7XG4gICAgICB9XG4gICAgICBpZiAoY2hlY2tDcm9zc0F4aXMpIHtcbiAgICAgICAgY29uc3QgbWluU2lkZSA9IGNyb3NzQXhpcyA9PT0gJ3knID8gJ3RvcCcgOiAnbGVmdCc7XG4gICAgICAgIGNvbnN0IG1heFNpZGUgPSBjcm9zc0F4aXMgPT09ICd5JyA/ICdib3R0b20nIDogJ3JpZ2h0JztcbiAgICAgICAgY29uc3QgbWluID0gY3Jvc3NBeGlzQ29vcmQgKyBvdmVyZmxvd1ttaW5TaWRlXTtcbiAgICAgICAgY29uc3QgbWF4ID0gY3Jvc3NBeGlzQ29vcmQgLSBvdmVyZmxvd1ttYXhTaWRlXTtcbiAgICAgICAgY3Jvc3NBeGlzQ29vcmQgPSBjbGFtcChtaW4sIGNyb3NzQXhpc0Nvb3JkLCBtYXgpO1xuICAgICAgfVxuICAgICAgY29uc3QgbGltaXRlZENvb3JkcyA9IGxpbWl0ZXIuZm4oe1xuICAgICAgICAuLi5zdGF0ZSxcbiAgICAgICAgW21haW5BeGlzXTogbWFpbkF4aXNDb29yZCxcbiAgICAgICAgW2Nyb3NzQXhpc106IGNyb3NzQXhpc0Nvb3JkXG4gICAgICB9KTtcbiAgICAgIHJldHVybiB7XG4gICAgICAgIC4uLmxpbWl0ZWRDb29yZHMsXG4gICAgICAgIGRhdGE6IHtcbiAgICAgICAgICB4OiBsaW1pdGVkQ29vcmRzLnggLSB4LFxuICAgICAgICAgIHk6IGxpbWl0ZWRDb29yZHMueSAtIHksXG4gICAgICAgICAgZW5hYmxlZDoge1xuICAgICAgICAgICAgW21haW5BeGlzXTogY2hlY2tNYWluQXhpcyxcbiAgICAgICAgICAgIFtjcm9zc0F4aXNdOiBjaGVja0Nyb3NzQXhpc1xuICAgICAgICAgIH1cbiAgICAgICAgfVxuICAgICAgfTtcbiAgICB9XG4gIH07XG59O1xuLyoqXG4gKiBCdWlsdC1pbiBgbGltaXRlcmAgdGhhdCB3aWxsIHN0b3AgYHNoaWZ0KClgIGF0IGEgY2VydGFpbiBwb2ludC5cbiAqL1xuY29uc3QgbGltaXRTaGlmdCA9IGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gIGlmIChvcHRpb25zID09PSB2b2lkIDApIHtcbiAgICBvcHRpb25zID0ge307XG4gIH1cbiAgcmV0dXJuIHtcbiAgICBvcHRpb25zLFxuICAgIGZuKHN0YXRlKSB7XG4gICAgICBjb25zdCB7XG4gICAgICAgIHgsXG4gICAgICAgIHksXG4gICAgICAgIHBsYWNlbWVudCxcbiAgICAgICAgcmVjdHMsXG4gICAgICAgIG1pZGRsZXdhcmVEYXRhXG4gICAgICB9ID0gc3RhdGU7XG4gICAgICBjb25zdCB7XG4gICAgICAgIG9mZnNldCA9IDAsXG4gICAgICAgIG1haW5BeGlzOiBjaGVja01haW5BeGlzID0gdHJ1ZSxcbiAgICAgICAgY3Jvc3NBeGlzOiBjaGVja0Nyb3NzQXhpcyA9IHRydWVcbiAgICAgIH0gPSBldmFsdWF0ZShvcHRpb25zLCBzdGF0ZSk7XG4gICAgICBjb25zdCBjb29yZHMgPSB7XG4gICAgICAgIHgsXG4gICAgICAgIHlcbiAgICAgIH07XG4gICAgICBjb25zdCBjcm9zc0F4aXMgPSBnZXRTaWRlQXhpcyhwbGFjZW1lbnQpO1xuICAgICAgY29uc3QgbWFpbkF4aXMgPSBnZXRPcHBvc2l0ZUF4aXMoY3Jvc3NBeGlzKTtcbiAgICAgIGxldCBtYWluQXhpc0Nvb3JkID0gY29vcmRzW21haW5BeGlzXTtcbiAgICAgIGxldCBjcm9zc0F4aXNDb29yZCA9IGNvb3Jkc1tjcm9zc0F4aXNdO1xuICAgICAgY29uc3QgcmF3T2Zmc2V0ID0gZXZhbHVhdGUob2Zmc2V0LCBzdGF0ZSk7XG4gICAgICBjb25zdCBjb21wdXRlZE9mZnNldCA9IHR5cGVvZiByYXdPZmZzZXQgPT09ICdudW1iZXInID8ge1xuICAgICAgICBtYWluQXhpczogcmF3T2Zmc2V0LFxuICAgICAgICBjcm9zc0F4aXM6IDBcbiAgICAgIH0gOiB7XG4gICAgICAgIG1haW5BeGlzOiAwLFxuICAgICAgICBjcm9zc0F4aXM6IDAsXG4gICAgICAgIC4uLnJhd09mZnNldFxuICAgICAgfTtcbiAgICAgIGlmIChjaGVja01haW5BeGlzKSB7XG4gICAgICAgIGNvbnN0IGxlbiA9IG1haW5BeGlzID09PSAneScgPyAnaGVpZ2h0JyA6ICd3aWR0aCc7XG4gICAgICAgIGNvbnN0IGxpbWl0TWluID0gcmVjdHMucmVmZXJlbmNlW21haW5BeGlzXSAtIHJlY3RzLmZsb2F0aW5nW2xlbl0gKyBjb21wdXRlZE9mZnNldC5tYWluQXhpcztcbiAgICAgICAgY29uc3QgbGltaXRNYXggPSByZWN0cy5yZWZlcmVuY2VbbWFpbkF4aXNdICsgcmVjdHMucmVmZXJlbmNlW2xlbl0gLSBjb21wdXRlZE9mZnNldC5tYWluQXhpcztcbiAgICAgICAgaWYgKG1haW5BeGlzQ29vcmQgPCBsaW1pdE1pbikge1xuICAgICAgICAgIG1haW5BeGlzQ29vcmQgPSBsaW1pdE1pbjtcbiAgICAgICAgfSBlbHNlIGlmIChtYWluQXhpc0Nvb3JkID4gbGltaXRNYXgpIHtcbiAgICAgICAgICBtYWluQXhpc0Nvb3JkID0gbGltaXRNYXg7XG4gICAgICAgIH1cbiAgICAgIH1cbiAgICAgIGlmIChjaGVja0Nyb3NzQXhpcykge1xuICAgICAgICB2YXIgX21pZGRsZXdhcmVEYXRhJG9mZnNlLCBfbWlkZGxld2FyZURhdGEkb2Zmc2UyO1xuICAgICAgICBjb25zdCBsZW4gPSBtYWluQXhpcyA9PT0gJ3knID8gJ3dpZHRoJyA6ICdoZWlnaHQnO1xuICAgICAgICBjb25zdCBpc09yaWdpblNpZGUgPSBbJ3RvcCcsICdsZWZ0J10uaW5jbHVkZXMoZ2V0U2lkZShwbGFjZW1lbnQpKTtcbiAgICAgICAgY29uc3QgbGltaXRNaW4gPSByZWN0cy5yZWZlcmVuY2VbY3Jvc3NBeGlzXSAtIHJlY3RzLmZsb2F0aW5nW2xlbl0gKyAoaXNPcmlnaW5TaWRlID8gKChfbWlkZGxld2FyZURhdGEkb2Zmc2UgPSBtaWRkbGV3YXJlRGF0YS5vZmZzZXQpID09IG51bGwgPyB2b2lkIDAgOiBfbWlkZGxld2FyZURhdGEkb2Zmc2VbY3Jvc3NBeGlzXSkgfHwgMCA6IDApICsgKGlzT3JpZ2luU2lkZSA/IDAgOiBjb21wdXRlZE9mZnNldC5jcm9zc0F4aXMpO1xuICAgICAgICBjb25zdCBsaW1pdE1heCA9IHJlY3RzLnJlZmVyZW5jZVtjcm9zc0F4aXNdICsgcmVjdHMucmVmZXJlbmNlW2xlbl0gKyAoaXNPcmlnaW5TaWRlID8gMCA6ICgoX21pZGRsZXdhcmVEYXRhJG9mZnNlMiA9IG1pZGRsZXdhcmVEYXRhLm9mZnNldCkgPT0gbnVsbCA/IHZvaWQgMCA6IF9taWRkbGV3YXJlRGF0YSRvZmZzZTJbY3Jvc3NBeGlzXSkgfHwgMCkgLSAoaXNPcmlnaW5TaWRlID8gY29tcHV0ZWRPZmZzZXQuY3Jvc3NBeGlzIDogMCk7XG4gICAgICAgIGlmIChjcm9zc0F4aXNDb29yZCA8IGxpbWl0TWluKSB7XG4gICAgICAgICAgY3Jvc3NBeGlzQ29vcmQgPSBsaW1pdE1pbjtcbiAgICAgICAgfSBlbHNlIGlmIChjcm9zc0F4aXNDb29yZCA+IGxpbWl0TWF4KSB7XG4gICAgICAgICAgY3Jvc3NBeGlzQ29vcmQgPSBsaW1pdE1heDtcbiAgICAgICAgfVxuICAgICAgfVxuICAgICAgcmV0dXJuIHtcbiAgICAgICAgW21haW5BeGlzXTogbWFpbkF4aXNDb29yZCxcbiAgICAgICAgW2Nyb3NzQXhpc106IGNyb3NzQXhpc0Nvb3JkXG4gICAgICB9O1xuICAgIH1cbiAgfTtcbn07XG5cbi8qKlxuICogUHJvdmlkZXMgZGF0YSB0aGF0IGFsbG93cyB5b3UgdG8gY2hhbmdlIHRoZSBzaXplIG9mIHRoZSBmbG9hdGluZyBlbGVtZW50IOKAlFxuICogZm9yIGluc3RhbmNlLCBwcmV2ZW50IGl0IGZyb20gb3ZlcmZsb3dpbmcgdGhlIGNsaXBwaW5nIGJvdW5kYXJ5IG9yIG1hdGNoIHRoZVxuICogd2lkdGggb2YgdGhlIHJlZmVyZW5jZSBlbGVtZW50LlxuICogQHNlZSBodHRwczovL2Zsb2F0aW5nLXVpLmNvbS9kb2NzL3NpemVcbiAqL1xuY29uc3Qgc2l6ZSA9IGZ1bmN0aW9uIChvcHRpb25zKSB7XG4gIGlmIChvcHRpb25zID09PSB2b2lkIDApIHtcbiAgICBvcHRpb25zID0ge307XG4gIH1cbiAgcmV0dXJuIHtcbiAgICBuYW1lOiAnc2l6ZScsXG4gICAgb3B0aW9ucyxcbiAgICBhc3luYyBmbihzdGF0ZSkge1xuICAgICAgdmFyIF9zdGF0ZSRtaWRkbGV3YXJlRGF0YSwgX3N0YXRlJG1pZGRsZXdhcmVEYXRhMjtcbiAgICAgIGNvbnN0IHtcbiAgICAgICAgcGxhY2VtZW50LFxuICAgICAgICByZWN0cyxcbiAgICAgICAgcGxhdGZvcm0sXG4gICAgICAgIGVsZW1lbnRzXG4gICAgICB9ID0gc3RhdGU7XG4gICAgICBjb25zdCB7XG4gICAgICAgIGFwcGx5ID0gKCkgPT4ge30sXG4gICAgICAgIC4uLmRldGVjdE92ZXJmbG93T3B0aW9uc1xuICAgICAgfSA9IGV2YWx1YXRlKG9wdGlvbnMsIHN0YXRlKTtcbiAgICAgIGNvbnN0IG92ZXJmbG93ID0gYXdhaXQgZGV0ZWN0T3ZlcmZsb3coc3RhdGUsIGRldGVjdE92ZXJmbG93T3B0aW9ucyk7XG4gICAgICBjb25zdCBzaWRlID0gZ2V0U2lkZShwbGFjZW1lbnQpO1xuICAgICAgY29uc3QgYWxpZ25tZW50ID0gZ2V0QWxpZ25tZW50KHBsYWNlbWVudCk7XG4gICAgICBjb25zdCBpc1lBeGlzID0gZ2V0U2lkZUF4aXMocGxhY2VtZW50KSA9PT0gJ3knO1xuICAgICAgY29uc3Qge1xuICAgICAgICB3aWR0aCxcbiAgICAgICAgaGVpZ2h0XG4gICAgICB9ID0gcmVjdHMuZmxvYXRpbmc7XG4gICAgICBsZXQgaGVpZ2h0U2lkZTtcbiAgICAgIGxldCB3aWR0aFNpZGU7XG4gICAgICBpZiAoc2lkZSA9PT0gJ3RvcCcgfHwgc2lkZSA9PT0gJ2JvdHRvbScpIHtcbiAgICAgICAgaGVpZ2h0U2lkZSA9IHNpZGU7XG4gICAgICAgIHdpZHRoU2lkZSA9IGFsaWdubWVudCA9PT0gKChhd2FpdCAocGxhdGZvcm0uaXNSVEwgPT0gbnVsbCA/IHZvaWQgMCA6IHBsYXRmb3JtLmlzUlRMKGVsZW1lbnRzLmZsb2F0aW5nKSkpID8gJ3N0YXJ0JyA6ICdlbmQnKSA/ICdsZWZ0JyA6ICdyaWdodCc7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICB3aWR0aFNpZGUgPSBzaWRlO1xuICAgICAgICBoZWlnaHRTaWRlID0gYWxpZ25tZW50ID09PSAnZW5kJyA/ICd0b3AnIDogJ2JvdHRvbSc7XG4gICAgICB9XG4gICAgICBjb25zdCBtYXhpbXVtQ2xpcHBpbmdIZWlnaHQgPSBoZWlnaHQgLSBvdmVyZmxvdy50b3AgLSBvdmVyZmxvdy5ib3R0b207XG4gICAgICBjb25zdCBtYXhpbXVtQ2xpcHBpbmdXaWR0aCA9IHdpZHRoIC0gb3ZlcmZsb3cubGVmdCAtIG92ZXJmbG93LnJpZ2h0O1xuICAgICAgY29uc3Qgb3ZlcmZsb3dBdmFpbGFibGVIZWlnaHQgPSBtaW4oaGVpZ2h0IC0gb3ZlcmZsb3dbaGVpZ2h0U2lkZV0sIG1heGltdW1DbGlwcGluZ0hlaWdodCk7XG4gICAgICBjb25zdCBvdmVyZmxvd0F2YWlsYWJsZVdpZHRoID0gbWluKHdpZHRoIC0gb3ZlcmZsb3dbd2lkdGhTaWRlXSwgbWF4aW11bUNsaXBwaW5nV2lkdGgpO1xuICAgICAgY29uc3Qgbm9TaGlmdCA9ICFzdGF0ZS5taWRkbGV3YXJlRGF0YS5zaGlmdDtcbiAgICAgIGxldCBhdmFpbGFibGVIZWlnaHQgPSBvdmVyZmxvd0F2YWlsYWJsZUhlaWdodDtcbiAgICAgIGxldCBhdmFpbGFibGVXaWR0aCA9IG92ZXJmbG93QXZhaWxhYmxlV2lkdGg7XG4gICAgICBpZiAoKF9zdGF0ZSRtaWRkbGV3YXJlRGF0YSA9IHN0YXRlLm1pZGRsZXdhcmVEYXRhLnNoaWZ0KSAhPSBudWxsICYmIF9zdGF0ZSRtaWRkbGV3YXJlRGF0YS5lbmFibGVkLngpIHtcbiAgICAgICAgYXZhaWxhYmxlV2lkdGggPSBtYXhpbXVtQ2xpcHBpbmdXaWR0aDtcbiAgICAgIH1cbiAgICAgIGlmICgoX3N0YXRlJG1pZGRsZXdhcmVEYXRhMiA9IHN0YXRlLm1pZGRsZXdhcmVEYXRhLnNoaWZ0KSAhPSBudWxsICYmIF9zdGF0ZSRtaWRkbGV3YXJlRGF0YTIuZW5hYmxlZC55KSB7XG4gICAgICAgIGF2YWlsYWJsZUhlaWdodCA9IG1heGltdW1DbGlwcGluZ0hlaWdodDtcbiAgICAgIH1cbiAgICAgIGlmIChub1NoaWZ0ICYmICFhbGlnbm1lbnQpIHtcbiAgICAgICAgY29uc3QgeE1pbiA9IG1heChvdmVyZmxvdy5sZWZ0LCAwKTtcbiAgICAgICAgY29uc3QgeE1heCA9IG1heChvdmVyZmxvdy5yaWdodCwgMCk7XG4gICAgICAgIGNvbnN0IHlNaW4gPSBtYXgob3ZlcmZsb3cudG9wLCAwKTtcbiAgICAgICAgY29uc3QgeU1heCA9IG1heChvdmVyZmxvdy5ib3R0b20sIDApO1xuICAgICAgICBpZiAoaXNZQXhpcykge1xuICAgICAgICAgIGF2YWlsYWJsZVdpZHRoID0gd2lkdGggLSAyICogKHhNaW4gIT09IDAgfHwgeE1heCAhPT0gMCA/IHhNaW4gKyB4TWF4IDogbWF4KG92ZXJmbG93LmxlZnQsIG92ZXJmbG93LnJpZ2h0KSk7XG4gICAgICAgIH0gZWxzZSB7XG4gICAgICAgICAgYXZhaWxhYmxlSGVpZ2h0ID0gaGVpZ2h0IC0gMiAqICh5TWluICE9PSAwIHx8IHlNYXggIT09IDAgPyB5TWluICsgeU1heCA6IG1heChvdmVyZmxvdy50b3AsIG92ZXJmbG93LmJvdHRvbSkpO1xuICAgICAgICB9XG4gICAgICB9XG4gICAgICBhd2FpdCBhcHBseSh7XG4gICAgICAgIC4uLnN0YXRlLFxuICAgICAgICBhdmFpbGFibGVXaWR0aCxcbiAgICAgICAgYXZhaWxhYmxlSGVpZ2h0XG4gICAgICB9KTtcbiAgICAgIGNvbnN0IG5leHREaW1lbnNpb25zID0gYXdhaXQgcGxhdGZvcm0uZ2V0RGltZW5zaW9ucyhlbGVtZW50cy5mbG9hdGluZyk7XG4gICAgICBpZiAod2lkdGggIT09IG5leHREaW1lbnNpb25zLndpZHRoIHx8IGhlaWdodCAhPT0gbmV4dERpbWVuc2lvbnMuaGVpZ2h0KSB7XG4gICAgICAgIHJldHVybiB7XG4gICAgICAgICAgcmVzZXQ6IHtcbiAgICAgICAgICAgIHJlY3RzOiB0cnVlXG4gICAgICAgICAgfVxuICAgICAgICB9O1xuICAgICAgfVxuICAgICAgcmV0dXJuIHt9O1xuICAgIH1cbiAgfTtcbn07XG5cbmV4cG9ydCB7IGFycm93LCBhdXRvUGxhY2VtZW50LCBjb21wdXRlUG9zaXRpb24sIGRldGVjdE92ZXJmbG93LCBmbGlwLCBoaWRlLCBpbmxpbmUsIGxpbWl0U2hpZnQsIG9mZnNldCwgc2hpZnQsIHNpemUgfTtcbiIsImZ1bmN0aW9uIGhhc1dpbmRvdygpIHtcbiAgcmV0dXJuIHR5cGVvZiB3aW5kb3cgIT09ICd1bmRlZmluZWQnO1xufVxuZnVuY3Rpb24gZ2V0Tm9kZU5hbWUobm9kZSkge1xuICBpZiAoaXNOb2RlKG5vZGUpKSB7XG4gICAgcmV0dXJuIChub2RlLm5vZGVOYW1lIHx8ICcnKS50b0xvd2VyQ2FzZSgpO1xuICB9XG4gIC8vIE1vY2tlZCBub2RlcyBpbiB0ZXN0aW5nIGVudmlyb25tZW50cyBtYXkgbm90IGJlIGluc3RhbmNlcyBvZiBOb2RlLiBCeVxuICAvLyByZXR1cm5pbmcgYCNkb2N1bWVudGAgYW4gaW5maW5pdGUgbG9vcCB3b24ndCBvY2N1ci5cbiAgLy8gaHR0cHM6Ly9naXRodWIuY29tL2Zsb2F0aW5nLXVpL2Zsb2F0aW5nLXVpL2lzc3Vlcy8yMzE3XG4gIHJldHVybiAnI2RvY3VtZW50Jztcbn1cbmZ1bmN0aW9uIGdldFdpbmRvdyhub2RlKSB7XG4gIHZhciBfbm9kZSRvd25lckRvY3VtZW50O1xuICByZXR1cm4gKG5vZGUgPT0gbnVsbCB8fCAoX25vZGUkb3duZXJEb2N1bWVudCA9IG5vZGUub3duZXJEb2N1bWVudCkgPT0gbnVsbCA/IHZvaWQgMCA6IF9ub2RlJG93bmVyRG9jdW1lbnQuZGVmYXVsdFZpZXcpIHx8IHdpbmRvdztcbn1cbmZ1bmN0aW9uIGdldERvY3VtZW50RWxlbWVudChub2RlKSB7XG4gIHZhciBfcmVmO1xuICByZXR1cm4gKF9yZWYgPSAoaXNOb2RlKG5vZGUpID8gbm9kZS5vd25lckRvY3VtZW50IDogbm9kZS5kb2N1bWVudCkgfHwgd2luZG93LmRvY3VtZW50KSA9PSBudWxsID8gdm9pZCAwIDogX3JlZi5kb2N1bWVudEVsZW1lbnQ7XG59XG5mdW5jdGlvbiBpc05vZGUodmFsdWUpIHtcbiAgaWYgKCFoYXNXaW5kb3coKSkge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuICByZXR1cm4gdmFsdWUgaW5zdGFuY2VvZiBOb2RlIHx8IHZhbHVlIGluc3RhbmNlb2YgZ2V0V2luZG93KHZhbHVlKS5Ob2RlO1xufVxuZnVuY3Rpb24gaXNFbGVtZW50KHZhbHVlKSB7XG4gIGlmICghaGFzV2luZG93KCkpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cbiAgcmV0dXJuIHZhbHVlIGluc3RhbmNlb2YgRWxlbWVudCB8fCB2YWx1ZSBpbnN0YW5jZW9mIGdldFdpbmRvdyh2YWx1ZSkuRWxlbWVudDtcbn1cbmZ1bmN0aW9uIGlzSFRNTEVsZW1lbnQodmFsdWUpIHtcbiAgaWYgKCFoYXNXaW5kb3coKSkge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuICByZXR1cm4gdmFsdWUgaW5zdGFuY2VvZiBIVE1MRWxlbWVudCB8fCB2YWx1ZSBpbnN0YW5jZW9mIGdldFdpbmRvdyh2YWx1ZSkuSFRNTEVsZW1lbnQ7XG59XG5mdW5jdGlvbiBpc1NoYWRvd1Jvb3QodmFsdWUpIHtcbiAgaWYgKCFoYXNXaW5kb3coKSB8fCB0eXBlb2YgU2hhZG93Um9vdCA9PT0gJ3VuZGVmaW5lZCcpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cbiAgcmV0dXJuIHZhbHVlIGluc3RhbmNlb2YgU2hhZG93Um9vdCB8fCB2YWx1ZSBpbnN0YW5jZW9mIGdldFdpbmRvdyh2YWx1ZSkuU2hhZG93Um9vdDtcbn1cbmZ1bmN0aW9uIGlzT3ZlcmZsb3dFbGVtZW50KGVsZW1lbnQpIHtcbiAgY29uc3Qge1xuICAgIG92ZXJmbG93LFxuICAgIG92ZXJmbG93WCxcbiAgICBvdmVyZmxvd1ksXG4gICAgZGlzcGxheVxuICB9ID0gZ2V0Q29tcHV0ZWRTdHlsZShlbGVtZW50KTtcbiAgcmV0dXJuIC9hdXRvfHNjcm9sbHxvdmVybGF5fGhpZGRlbnxjbGlwLy50ZXN0KG92ZXJmbG93ICsgb3ZlcmZsb3dZICsgb3ZlcmZsb3dYKSAmJiAhWydpbmxpbmUnLCAnY29udGVudHMnXS5pbmNsdWRlcyhkaXNwbGF5KTtcbn1cbmZ1bmN0aW9uIGlzVGFibGVFbGVtZW50KGVsZW1lbnQpIHtcbiAgcmV0dXJuIFsndGFibGUnLCAndGQnLCAndGgnXS5pbmNsdWRlcyhnZXROb2RlTmFtZShlbGVtZW50KSk7XG59XG5mdW5jdGlvbiBpc1RvcExheWVyKGVsZW1lbnQpIHtcbiAgcmV0dXJuIFsnOnBvcG92ZXItb3BlbicsICc6bW9kYWwnXS5zb21lKHNlbGVjdG9yID0+IHtcbiAgICB0cnkge1xuICAgICAgcmV0dXJuIGVsZW1lbnQubWF0Y2hlcyhzZWxlY3Rvcik7XG4gICAgfSBjYXRjaCAoZSkge1xuICAgICAgcmV0dXJuIGZhbHNlO1xuICAgIH1cbiAgfSk7XG59XG5mdW5jdGlvbiBpc0NvbnRhaW5pbmdCbG9jayhlbGVtZW50T3JDc3MpIHtcbiAgY29uc3Qgd2Via2l0ID0gaXNXZWJLaXQoKTtcbiAgY29uc3QgY3NzID0gaXNFbGVtZW50KGVsZW1lbnRPckNzcykgPyBnZXRDb21wdXRlZFN0eWxlKGVsZW1lbnRPckNzcykgOiBlbGVtZW50T3JDc3M7XG5cbiAgLy8gaHR0cHM6Ly9kZXZlbG9wZXIubW96aWxsYS5vcmcvZW4tVVMvZG9jcy9XZWIvQ1NTL0NvbnRhaW5pbmdfYmxvY2sjaWRlbnRpZnlpbmdfdGhlX2NvbnRhaW5pbmdfYmxvY2tcbiAgcmV0dXJuIGNzcy50cmFuc2Zvcm0gIT09ICdub25lJyB8fCBjc3MucGVyc3BlY3RpdmUgIT09ICdub25lJyB8fCAoY3NzLmNvbnRhaW5lclR5cGUgPyBjc3MuY29udGFpbmVyVHlwZSAhPT0gJ25vcm1hbCcgOiBmYWxzZSkgfHwgIXdlYmtpdCAmJiAoY3NzLmJhY2tkcm9wRmlsdGVyID8gY3NzLmJhY2tkcm9wRmlsdGVyICE9PSAnbm9uZScgOiBmYWxzZSkgfHwgIXdlYmtpdCAmJiAoY3NzLmZpbHRlciA/IGNzcy5maWx0ZXIgIT09ICdub25lJyA6IGZhbHNlKSB8fCBbJ3RyYW5zZm9ybScsICdwZXJzcGVjdGl2ZScsICdmaWx0ZXInXS5zb21lKHZhbHVlID0+IChjc3Mud2lsbENoYW5nZSB8fCAnJykuaW5jbHVkZXModmFsdWUpKSB8fCBbJ3BhaW50JywgJ2xheW91dCcsICdzdHJpY3QnLCAnY29udGVudCddLnNvbWUodmFsdWUgPT4gKGNzcy5jb250YWluIHx8ICcnKS5pbmNsdWRlcyh2YWx1ZSkpO1xufVxuZnVuY3Rpb24gZ2V0Q29udGFpbmluZ0Jsb2NrKGVsZW1lbnQpIHtcbiAgbGV0IGN1cnJlbnROb2RlID0gZ2V0UGFyZW50Tm9kZShlbGVtZW50KTtcbiAgd2hpbGUgKGlzSFRNTEVsZW1lbnQoY3VycmVudE5vZGUpICYmICFpc0xhc3RUcmF2ZXJzYWJsZU5vZGUoY3VycmVudE5vZGUpKSB7XG4gICAgaWYgKGlzQ29udGFpbmluZ0Jsb2NrKGN1cnJlbnROb2RlKSkge1xuICAgICAgcmV0dXJuIGN1cnJlbnROb2RlO1xuICAgIH0gZWxzZSBpZiAoaXNUb3BMYXllcihjdXJyZW50Tm9kZSkpIHtcbiAgICAgIHJldHVybiBudWxsO1xuICAgIH1cbiAgICBjdXJyZW50Tm9kZSA9IGdldFBhcmVudE5vZGUoY3VycmVudE5vZGUpO1xuICB9XG4gIHJldHVybiBudWxsO1xufVxuZnVuY3Rpb24gaXNXZWJLaXQoKSB7XG4gIGlmICh0eXBlb2YgQ1NTID09PSAndW5kZWZpbmVkJyB8fCAhQ1NTLnN1cHBvcnRzKSByZXR1cm4gZmFsc2U7XG4gIHJldHVybiBDU1Muc3VwcG9ydHMoJy13ZWJraXQtYmFja2Ryb3AtZmlsdGVyJywgJ25vbmUnKTtcbn1cbmZ1bmN0aW9uIGlzTGFzdFRyYXZlcnNhYmxlTm9kZShub2RlKSB7XG4gIHJldHVybiBbJ2h0bWwnLCAnYm9keScsICcjZG9jdW1lbnQnXS5pbmNsdWRlcyhnZXROb2RlTmFtZShub2RlKSk7XG59XG5mdW5jdGlvbiBnZXRDb21wdXRlZFN0eWxlKGVsZW1lbnQpIHtcbiAgcmV0dXJuIGdldFdpbmRvdyhlbGVtZW50KS5nZXRDb21wdXRlZFN0eWxlKGVsZW1lbnQpO1xufVxuZnVuY3Rpb24gZ2V0Tm9kZVNjcm9sbChlbGVtZW50KSB7XG4gIGlmIChpc0VsZW1lbnQoZWxlbWVudCkpIHtcbiAgICByZXR1cm4ge1xuICAgICAgc2Nyb2xsTGVmdDogZWxlbWVudC5zY3JvbGxMZWZ0LFxuICAgICAgc2Nyb2xsVG9wOiBlbGVtZW50LnNjcm9sbFRvcFxuICAgIH07XG4gIH1cbiAgcmV0dXJuIHtcbiAgICBzY3JvbGxMZWZ0OiBlbGVtZW50LnNjcm9sbFgsXG4gICAgc2Nyb2xsVG9wOiBlbGVtZW50LnNjcm9sbFlcbiAgfTtcbn1cbmZ1bmN0aW9uIGdldFBhcmVudE5vZGUobm9kZSkge1xuICBpZiAoZ2V0Tm9kZU5hbWUobm9kZSkgPT09ICdodG1sJykge1xuICAgIHJldHVybiBub2RlO1xuICB9XG4gIGNvbnN0IHJlc3VsdCA9XG4gIC8vIFN0ZXAgaW50byB0aGUgc2hhZG93IERPTSBvZiB0aGUgcGFyZW50IG9mIGEgc2xvdHRlZCBub2RlLlxuICBub2RlLmFzc2lnbmVkU2xvdCB8fFxuICAvLyBET00gRWxlbWVudCBkZXRlY3RlZC5cbiAgbm9kZS5wYXJlbnROb2RlIHx8XG4gIC8vIFNoYWRvd1Jvb3QgZGV0ZWN0ZWQuXG4gIGlzU2hhZG93Um9vdChub2RlKSAmJiBub2RlLmhvc3QgfHxcbiAgLy8gRmFsbGJhY2suXG4gIGdldERvY3VtZW50RWxlbWVudChub2RlKTtcbiAgcmV0dXJuIGlzU2hhZG93Um9vdChyZXN1bHQpID8gcmVzdWx0Lmhvc3QgOiByZXN1bHQ7XG59XG5mdW5jdGlvbiBnZXROZWFyZXN0T3ZlcmZsb3dBbmNlc3Rvcihub2RlKSB7XG4gIGNvbnN0IHBhcmVudE5vZGUgPSBnZXRQYXJlbnROb2RlKG5vZGUpO1xuICBpZiAoaXNMYXN0VHJhdmVyc2FibGVOb2RlKHBhcmVudE5vZGUpKSB7XG4gICAgcmV0dXJuIG5vZGUub3duZXJEb2N1bWVudCA/IG5vZGUub3duZXJEb2N1bWVudC5ib2R5IDogbm9kZS5ib2R5O1xuICB9XG4gIGlmIChpc0hUTUxFbGVtZW50KHBhcmVudE5vZGUpICYmIGlzT3ZlcmZsb3dFbGVtZW50KHBhcmVudE5vZGUpKSB7XG4gICAgcmV0dXJuIHBhcmVudE5vZGU7XG4gIH1cbiAgcmV0dXJuIGdldE5lYXJlc3RPdmVyZmxvd0FuY2VzdG9yKHBhcmVudE5vZGUpO1xufVxuZnVuY3Rpb24gZ2V0T3ZlcmZsb3dBbmNlc3RvcnMobm9kZSwgbGlzdCwgdHJhdmVyc2VJZnJhbWVzKSB7XG4gIHZhciBfbm9kZSRvd25lckRvY3VtZW50MjtcbiAgaWYgKGxpc3QgPT09IHZvaWQgMCkge1xuICAgIGxpc3QgPSBbXTtcbiAgfVxuICBpZiAodHJhdmVyc2VJZnJhbWVzID09PSB2b2lkIDApIHtcbiAgICB0cmF2ZXJzZUlmcmFtZXMgPSB0cnVlO1xuICB9XG4gIGNvbnN0IHNjcm9sbGFibGVBbmNlc3RvciA9IGdldE5lYXJlc3RPdmVyZmxvd0FuY2VzdG9yKG5vZGUpO1xuICBjb25zdCBpc0JvZHkgPSBzY3JvbGxhYmxlQW5jZXN0b3IgPT09ICgoX25vZGUkb3duZXJEb2N1bWVudDIgPSBub2RlLm93bmVyRG9jdW1lbnQpID09IG51bGwgPyB2b2lkIDAgOiBfbm9kZSRvd25lckRvY3VtZW50Mi5ib2R5KTtcbiAgY29uc3Qgd2luID0gZ2V0V2luZG93KHNjcm9sbGFibGVBbmNlc3Rvcik7XG4gIGlmIChpc0JvZHkpIHtcbiAgICBjb25zdCBmcmFtZUVsZW1lbnQgPSBnZXRGcmFtZUVsZW1lbnQod2luKTtcbiAgICByZXR1cm4gbGlzdC5jb25jYXQod2luLCB3aW4udmlzdWFsVmlld3BvcnQgfHwgW10sIGlzT3ZlcmZsb3dFbGVtZW50KHNjcm9sbGFibGVBbmNlc3RvcikgPyBzY3JvbGxhYmxlQW5jZXN0b3IgOiBbXSwgZnJhbWVFbGVtZW50ICYmIHRyYXZlcnNlSWZyYW1lcyA/IGdldE92ZXJmbG93QW5jZXN0b3JzKGZyYW1lRWxlbWVudCkgOiBbXSk7XG4gIH1cbiAgcmV0dXJuIGxpc3QuY29uY2F0KHNjcm9sbGFibGVBbmNlc3RvciwgZ2V0T3ZlcmZsb3dBbmNlc3RvcnMoc2Nyb2xsYWJsZUFuY2VzdG9yLCBbXSwgdHJhdmVyc2VJZnJhbWVzKSk7XG59XG5mdW5jdGlvbiBnZXRGcmFtZUVsZW1lbnQod2luKSB7XG4gIHJldHVybiB3aW4ucGFyZW50ICYmIE9iamVjdC5nZXRQcm90b3R5cGVPZih3aW4ucGFyZW50KSA/IHdpbi5mcmFtZUVsZW1lbnQgOiBudWxsO1xufVxuXG5leHBvcnQgeyBnZXRDb21wdXRlZFN0eWxlLCBnZXRDb250YWluaW5nQmxvY2ssIGdldERvY3VtZW50RWxlbWVudCwgZ2V0RnJhbWVFbGVtZW50LCBnZXROZWFyZXN0T3ZlcmZsb3dBbmNlc3RvciwgZ2V0Tm9kZU5hbWUsIGdldE5vZGVTY3JvbGwsIGdldE92ZXJmbG93QW5jZXN0b3JzLCBnZXRQYXJlbnROb2RlLCBnZXRXaW5kb3csIGlzQ29udGFpbmluZ0Jsb2NrLCBpc0VsZW1lbnQsIGlzSFRNTEVsZW1lbnQsIGlzTGFzdFRyYXZlcnNhYmxlTm9kZSwgaXNOb2RlLCBpc092ZXJmbG93RWxlbWVudCwgaXNTaGFkb3dSb290LCBpc1RhYmxlRWxlbWVudCwgaXNUb3BMYXllciwgaXNXZWJLaXQgfTtcbiIsIi8qKlxuICogQ3VzdG9tIHBvc2l0aW9uaW5nIHJlZmVyZW5jZSBlbGVtZW50LlxuICogQHNlZSBodHRwczovL2Zsb2F0aW5nLXVpLmNvbS9kb2NzL3ZpcnR1YWwtZWxlbWVudHNcbiAqL1xuXG5jb25zdCBzaWRlcyA9IFsndG9wJywgJ3JpZ2h0JywgJ2JvdHRvbScsICdsZWZ0J107XG5jb25zdCBhbGlnbm1lbnRzID0gWydzdGFydCcsICdlbmQnXTtcbmNvbnN0IHBsYWNlbWVudHMgPSAvKiNfX1BVUkVfXyovc2lkZXMucmVkdWNlKChhY2MsIHNpZGUpID0+IGFjYy5jb25jYXQoc2lkZSwgc2lkZSArIFwiLVwiICsgYWxpZ25tZW50c1swXSwgc2lkZSArIFwiLVwiICsgYWxpZ25tZW50c1sxXSksIFtdKTtcbmNvbnN0IG1pbiA9IE1hdGgubWluO1xuY29uc3QgbWF4ID0gTWF0aC5tYXg7XG5jb25zdCByb3VuZCA9IE1hdGgucm91bmQ7XG5jb25zdCBmbG9vciA9IE1hdGguZmxvb3I7XG5jb25zdCBjcmVhdGVDb29yZHMgPSB2ID0+ICh7XG4gIHg6IHYsXG4gIHk6IHZcbn0pO1xuY29uc3Qgb3Bwb3NpdGVTaWRlTWFwID0ge1xuICBsZWZ0OiAncmlnaHQnLFxuICByaWdodDogJ2xlZnQnLFxuICBib3R0b206ICd0b3AnLFxuICB0b3A6ICdib3R0b20nXG59O1xuY29uc3Qgb3Bwb3NpdGVBbGlnbm1lbnRNYXAgPSB7XG4gIHN0YXJ0OiAnZW5kJyxcbiAgZW5kOiAnc3RhcnQnXG59O1xuZnVuY3Rpb24gY2xhbXAoc3RhcnQsIHZhbHVlLCBlbmQpIHtcbiAgcmV0dXJuIG1heChzdGFydCwgbWluKHZhbHVlLCBlbmQpKTtcbn1cbmZ1bmN0aW9uIGV2YWx1YXRlKHZhbHVlLCBwYXJhbSkge1xuICByZXR1cm4gdHlwZW9mIHZhbHVlID09PSAnZnVuY3Rpb24nID8gdmFsdWUocGFyYW0pIDogdmFsdWU7XG59XG5mdW5jdGlvbiBnZXRTaWRlKHBsYWNlbWVudCkge1xuICByZXR1cm4gcGxhY2VtZW50LnNwbGl0KCctJylbMF07XG59XG5mdW5jdGlvbiBnZXRBbGlnbm1lbnQocGxhY2VtZW50KSB7XG4gIHJldHVybiBwbGFjZW1lbnQuc3BsaXQoJy0nKVsxXTtcbn1cbmZ1bmN0aW9uIGdldE9wcG9zaXRlQXhpcyhheGlzKSB7XG4gIHJldHVybiBheGlzID09PSAneCcgPyAneScgOiAneCc7XG59XG5mdW5jdGlvbiBnZXRBeGlzTGVuZ3RoKGF4aXMpIHtcbiAgcmV0dXJuIGF4aXMgPT09ICd5JyA/ICdoZWlnaHQnIDogJ3dpZHRoJztcbn1cbmZ1bmN0aW9uIGdldFNpZGVBeGlzKHBsYWNlbWVudCkge1xuICByZXR1cm4gWyd0b3AnLCAnYm90dG9tJ10uaW5jbHVkZXMoZ2V0U2lkZShwbGFjZW1lbnQpKSA/ICd5JyA6ICd4Jztcbn1cbmZ1bmN0aW9uIGdldEFsaWdubWVudEF4aXMocGxhY2VtZW50KSB7XG4gIHJldHVybiBnZXRPcHBvc2l0ZUF4aXMoZ2V0U2lkZUF4aXMocGxhY2VtZW50KSk7XG59XG5mdW5jdGlvbiBnZXRBbGlnbm1lbnRTaWRlcyhwbGFjZW1lbnQsIHJlY3RzLCBydGwpIHtcbiAgaWYgKHJ0bCA9PT0gdm9pZCAwKSB7XG4gICAgcnRsID0gZmFsc2U7XG4gIH1cbiAgY29uc3QgYWxpZ25tZW50ID0gZ2V0QWxpZ25tZW50KHBsYWNlbWVudCk7XG4gIGNvbnN0IGFsaWdubWVudEF4aXMgPSBnZXRBbGlnbm1lbnRBeGlzKHBsYWNlbWVudCk7XG4gIGNvbnN0IGxlbmd0aCA9IGdldEF4aXNMZW5ndGgoYWxpZ25tZW50QXhpcyk7XG4gIGxldCBtYWluQWxpZ25tZW50U2lkZSA9IGFsaWdubWVudEF4aXMgPT09ICd4JyA/IGFsaWdubWVudCA9PT0gKHJ0bCA/ICdlbmQnIDogJ3N0YXJ0JykgPyAncmlnaHQnIDogJ2xlZnQnIDogYWxpZ25tZW50ID09PSAnc3RhcnQnID8gJ2JvdHRvbScgOiAndG9wJztcbiAgaWYgKHJlY3RzLnJlZmVyZW5jZVtsZW5ndGhdID4gcmVjdHMuZmxvYXRpbmdbbGVuZ3RoXSkge1xuICAgIG1haW5BbGlnbm1lbnRTaWRlID0gZ2V0T3Bwb3NpdGVQbGFjZW1lbnQobWFpbkFsaWdubWVudFNpZGUpO1xuICB9XG4gIHJldHVybiBbbWFpbkFsaWdubWVudFNpZGUsIGdldE9wcG9zaXRlUGxhY2VtZW50KG1haW5BbGlnbm1lbnRTaWRlKV07XG59XG5mdW5jdGlvbiBnZXRFeHBhbmRlZFBsYWNlbWVudHMocGxhY2VtZW50KSB7XG4gIGNvbnN0IG9wcG9zaXRlUGxhY2VtZW50ID0gZ2V0T3Bwb3NpdGVQbGFjZW1lbnQocGxhY2VtZW50KTtcbiAgcmV0dXJuIFtnZXRPcHBvc2l0ZUFsaWdubWVudFBsYWNlbWVudChwbGFjZW1lbnQpLCBvcHBvc2l0ZVBsYWNlbWVudCwgZ2V0T3Bwb3NpdGVBbGlnbm1lbnRQbGFjZW1lbnQob3Bwb3NpdGVQbGFjZW1lbnQpXTtcbn1cbmZ1bmN0aW9uIGdldE9wcG9zaXRlQWxpZ25tZW50UGxhY2VtZW50KHBsYWNlbWVudCkge1xuICByZXR1cm4gcGxhY2VtZW50LnJlcGxhY2UoL3N0YXJ0fGVuZC9nLCBhbGlnbm1lbnQgPT4gb3Bwb3NpdGVBbGlnbm1lbnRNYXBbYWxpZ25tZW50XSk7XG59XG5mdW5jdGlvbiBnZXRTaWRlTGlzdChzaWRlLCBpc1N0YXJ0LCBydGwpIHtcbiAgY29uc3QgbHIgPSBbJ2xlZnQnLCAncmlnaHQnXTtcbiAgY29uc3QgcmwgPSBbJ3JpZ2h0JywgJ2xlZnQnXTtcbiAgY29uc3QgdGIgPSBbJ3RvcCcsICdib3R0b20nXTtcbiAgY29uc3QgYnQgPSBbJ2JvdHRvbScsICd0b3AnXTtcbiAgc3dpdGNoIChzaWRlKSB7XG4gICAgY2FzZSAndG9wJzpcbiAgICBjYXNlICdib3R0b20nOlxuICAgICAgaWYgKHJ0bCkgcmV0dXJuIGlzU3RhcnQgPyBybCA6IGxyO1xuICAgICAgcmV0dXJuIGlzU3RhcnQgPyBsciA6IHJsO1xuICAgIGNhc2UgJ2xlZnQnOlxuICAgIGNhc2UgJ3JpZ2h0JzpcbiAgICAgIHJldHVybiBpc1N0YXJ0ID8gdGIgOiBidDtcbiAgICBkZWZhdWx0OlxuICAgICAgcmV0dXJuIFtdO1xuICB9XG59XG5mdW5jdGlvbiBnZXRPcHBvc2l0ZUF4aXNQbGFjZW1lbnRzKHBsYWNlbWVudCwgZmxpcEFsaWdubWVudCwgZGlyZWN0aW9uLCBydGwpIHtcbiAgY29uc3QgYWxpZ25tZW50ID0gZ2V0QWxpZ25tZW50KHBsYWNlbWVudCk7XG4gIGxldCBsaXN0ID0gZ2V0U2lkZUxpc3QoZ2V0U2lkZShwbGFjZW1lbnQpLCBkaXJlY3Rpb24gPT09ICdzdGFydCcsIHJ0bCk7XG4gIGlmIChhbGlnbm1lbnQpIHtcbiAgICBsaXN0ID0gbGlzdC5tYXAoc2lkZSA9PiBzaWRlICsgXCItXCIgKyBhbGlnbm1lbnQpO1xuICAgIGlmIChmbGlwQWxpZ25tZW50KSB7XG4gICAgICBsaXN0ID0gbGlzdC5jb25jYXQobGlzdC5tYXAoZ2V0T3Bwb3NpdGVBbGlnbm1lbnRQbGFjZW1lbnQpKTtcbiAgICB9XG4gIH1cbiAgcmV0dXJuIGxpc3Q7XG59XG5mdW5jdGlvbiBnZXRPcHBvc2l0ZVBsYWNlbWVudChwbGFjZW1lbnQpIHtcbiAgcmV0dXJuIHBsYWNlbWVudC5yZXBsYWNlKC9sZWZ0fHJpZ2h0fGJvdHRvbXx0b3AvZywgc2lkZSA9PiBvcHBvc2l0ZVNpZGVNYXBbc2lkZV0pO1xufVxuZnVuY3Rpb24gZXhwYW5kUGFkZGluZ09iamVjdChwYWRkaW5nKSB7XG4gIHJldHVybiB7XG4gICAgdG9wOiAwLFxuICAgIHJpZ2h0OiAwLFxuICAgIGJvdHRvbTogMCxcbiAgICBsZWZ0OiAwLFxuICAgIC4uLnBhZGRpbmdcbiAgfTtcbn1cbmZ1bmN0aW9uIGdldFBhZGRpbmdPYmplY3QocGFkZGluZykge1xuICByZXR1cm4gdHlwZW9mIHBhZGRpbmcgIT09ICdudW1iZXInID8gZXhwYW5kUGFkZGluZ09iamVjdChwYWRkaW5nKSA6IHtcbiAgICB0b3A6IHBhZGRpbmcsXG4gICAgcmlnaHQ6IHBhZGRpbmcsXG4gICAgYm90dG9tOiBwYWRkaW5nLFxuICAgIGxlZnQ6IHBhZGRpbmdcbiAgfTtcbn1cbmZ1bmN0aW9uIHJlY3RUb0NsaWVudFJlY3QocmVjdCkge1xuICBjb25zdCB7XG4gICAgeCxcbiAgICB5LFxuICAgIHdpZHRoLFxuICAgIGhlaWdodFxuICB9ID0gcmVjdDtcbiAgcmV0dXJuIHtcbiAgICB3aWR0aCxcbiAgICBoZWlnaHQsXG4gICAgdG9wOiB5LFxuICAgIGxlZnQ6IHgsXG4gICAgcmlnaHQ6IHggKyB3aWR0aCxcbiAgICBib3R0b206IHkgKyBoZWlnaHQsXG4gICAgeCxcbiAgICB5XG4gIH07XG59XG5cbmV4cG9ydCB7IGFsaWdubWVudHMsIGNsYW1wLCBjcmVhdGVDb29yZHMsIGV2YWx1YXRlLCBleHBhbmRQYWRkaW5nT2JqZWN0LCBmbG9vciwgZ2V0QWxpZ25tZW50LCBnZXRBbGlnbm1lbnRBeGlzLCBnZXRBbGlnbm1lbnRTaWRlcywgZ2V0QXhpc0xlbmd0aCwgZ2V0RXhwYW5kZWRQbGFjZW1lbnRzLCBnZXRPcHBvc2l0ZUFsaWdubWVudFBsYWNlbWVudCwgZ2V0T3Bwb3NpdGVBeGlzLCBnZXRPcHBvc2l0ZUF4aXNQbGFjZW1lbnRzLCBnZXRPcHBvc2l0ZVBsYWNlbWVudCwgZ2V0UGFkZGluZ09iamVjdCwgZ2V0U2lkZSwgZ2V0U2lkZUF4aXMsIG1heCwgbWluLCBwbGFjZW1lbnRzLCByZWN0VG9DbGllbnRSZWN0LCByb3VuZCwgc2lkZXMgfTtcbiIsIi8vIFRoZSBtb2R1bGUgY2FjaGVcbnZhciBfX3dlYnBhY2tfbW9kdWxlX2NhY2hlX18gPSB7fTtcblxuLy8gVGhlIHJlcXVpcmUgZnVuY3Rpb25cbmZ1bmN0aW9uIF9fd2VicGFja19yZXF1aXJlX18obW9kdWxlSWQpIHtcblx0Ly8gQ2hlY2sgaWYgbW9kdWxlIGlzIGluIGNhY2hlXG5cdHZhciBjYWNoZWRNb2R1bGUgPSBfX3dlYnBhY2tfbW9kdWxlX2NhY2hlX19bbW9kdWxlSWRdO1xuXHRpZiAoY2FjaGVkTW9kdWxlICE9PSB1bmRlZmluZWQpIHtcblx0XHRyZXR1cm4gY2FjaGVkTW9kdWxlLmV4cG9ydHM7XG5cdH1cblx0Ly8gQ3JlYXRlIGEgbmV3IG1vZHVsZSAoYW5kIHB1dCBpdCBpbnRvIHRoZSBjYWNoZSlcblx0dmFyIG1vZHVsZSA9IF9fd2VicGFja19tb2R1bGVfY2FjaGVfX1ttb2R1bGVJZF0gPSB7XG5cdFx0Ly8gbm8gbW9kdWxlLmlkIG5lZWRlZFxuXHRcdC8vIG5vIG1vZHVsZS5sb2FkZWQgbmVlZGVkXG5cdFx0ZXhwb3J0czoge31cblx0fTtcblxuXHQvLyBFeGVjdXRlIHRoZSBtb2R1bGUgZnVuY3Rpb25cblx0X193ZWJwYWNrX21vZHVsZXNfX1ttb2R1bGVJZF0obW9kdWxlLCBtb2R1bGUuZXhwb3J0cywgX193ZWJwYWNrX3JlcXVpcmVfXyk7XG5cblx0Ly8gUmV0dXJuIHRoZSBleHBvcnRzIG9mIHRoZSBtb2R1bGVcblx0cmV0dXJuIG1vZHVsZS5leHBvcnRzO1xufVxuXG4iLCIvLyBkZWZpbmUgZ2V0dGVyIGZ1bmN0aW9ucyBmb3IgaGFybW9ueSBleHBvcnRzXG5fX3dlYnBhY2tfcmVxdWlyZV9fLmQgPSAoZXhwb3J0cywgZGVmaW5pdGlvbikgPT4ge1xuXHRmb3IodmFyIGtleSBpbiBkZWZpbml0aW9uKSB7XG5cdFx0aWYoX193ZWJwYWNrX3JlcXVpcmVfXy5vKGRlZmluaXRpb24sIGtleSkgJiYgIV9fd2VicGFja19yZXF1aXJlX18ubyhleHBvcnRzLCBrZXkpKSB7XG5cdFx0XHRPYmplY3QuZGVmaW5lUHJvcGVydHkoZXhwb3J0cywga2V5LCB7IGVudW1lcmFibGU6IHRydWUsIGdldDogZGVmaW5pdGlvbltrZXldIH0pO1xuXHRcdH1cblx0fVxufTsiLCJfX3dlYnBhY2tfcmVxdWlyZV9fLm8gPSAob2JqLCBwcm9wKSA9PiAoT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKG9iaiwgcHJvcCkpIiwiLy8gZGVmaW5lIF9fZXNNb2R1bGUgb24gZXhwb3J0c1xuX193ZWJwYWNrX3JlcXVpcmVfXy5yID0gKGV4cG9ydHMpID0+IHtcblx0aWYodHlwZW9mIFN5bWJvbCAhPT0gJ3VuZGVmaW5lZCcgJiYgU3ltYm9sLnRvU3RyaW5nVGFnKSB7XG5cdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KGV4cG9ydHMsIFN5bWJvbC50b1N0cmluZ1RhZywgeyB2YWx1ZTogJ01vZHVsZScgfSk7XG5cdH1cblx0T2JqZWN0LmRlZmluZVByb3BlcnR5KGV4cG9ydHMsICdfX2VzTW9kdWxlJywgeyB2YWx1ZTogdHJ1ZSB9KTtcbn07IiwiaW1wb3J0IEFscGluZSBmcm9tICdhbHBpbmVqcydcbmltcG9ydCBmaWx0ZXJlZE92ZXJ2aWV3IGZyb20gJy4vZmlsdGVyZWRPdmVydmlldyc7XG5pbXBvcnQgY29weVRvQ2xpcGJvYXJkQnV0dG9uIGZyb20gXCIuL2NvcHlUb0NsaXBib2FyZEJ1dHRvblwiO1xuaW1wb3J0IG1vZGFsQnV0dG9uIGZyb20gJy4vbW9kYWxCdXR0b24nO1xuaW1wb3J0IGNsYXNzSG92ZXIgZnJvbSAnLi9jbGFzc0hvdmVyJztcbmltcG9ydCBhamF4Rm9ybSBmcm9tICcuL2FqYXhGb3JtJztcbmltcG9ydCB0b29sdGlwIGZyb20gXCIuL3Rvb2x0aXBcIjtcbmltcG9ydCBzZXJ2ZXJTaWRlT3ZlcnZpZXcgZnJvbSAnLi9zZXJ2ZXJTaWRlT3ZlcnZpZXcnO1xuaW1wb3J0IGN1cnNvckltYWdlIGZyb20gJy4vY3Vyc29ySW1hZ2UnO1xuaW1wb3J0IGltcG9ydE1vZGFsIGZyb20gJy4vY29tcG9uZW50cy9pbXBvcnRNb2RhbCc7XG5pbXBvcnQgdG9nZ2xlciBmcm9tICcuL3V0aWxzL3RvZ2dsZXInO1xuaW1wb3J0IGFuY2hvckhvdmVyIGZyb20gJy4vYW5jaG9ySG92ZXInO1xuaW1wb3J0IGxpc3RNYW5hZ2VyIGZyb20gJy4vY29tcG9uZW50cy9saXN0TWFuYWdlcic7XG5pbXBvcnQgZGVja0NvbGxlY3Rpb24gZnJvbSAnLi9kZWNrQ29sbGVjdGlvbic7XG4gXG4od2luZG93IGFzIGFueSkuQWxwaW5lID0gQWxwaW5lO1xuXG5BbHBpbmUuZGF0YSgnZmlsdGVyZWRPdmVydmlldycsIGZpbHRlcmVkT3ZlcnZpZXcpO1xuQWxwaW5lLmRhdGEoJ3NlcnZlclNpZGVPdmVydmlldycsIHNlcnZlclNpZGVPdmVydmlldyk7XG5BbHBpbmUuZGF0YShcImltcG9ydE1vZGFsXCIsIGltcG9ydE1vZGFsKTtcblxuQWxwaW5lLnN0YXJ0KClcblxuY29weVRvQ2xpcGJvYXJkQnV0dG9uKCk7XG5tb2RhbEJ1dHRvbigpO1xuY2xhc3NIb3ZlcigpO1xuYWpheEZvcm0oKTtcbnRvb2x0aXAoKTtcbmN1cnNvckltYWdlKCk7XG50b2dnbGVyKCk7XG5hbmNob3JIb3ZlcigpO1xubGlzdE1hbmFnZXIoKTtcbmRlY2tDb2xsZWN0aW9uKCk7Il0sIm5hbWVzIjpbXSwic291cmNlUm9vdCI6IiJ9