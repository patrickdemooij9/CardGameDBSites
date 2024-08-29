/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const t=window,i=t.ShadowRoot&&(void 0===t.ShadyCSS||t.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,s=Symbol(),e=new WeakMap;let r=class{constructor(t,i,e){if(this._$cssResult$=!0,e!==s)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=i}get styleSheet(){let t=this.o;const s=this.t;if(i&&void 0===t){const i=void 0!==s&&1===s.length;i&&(t=e.get(s)),void 0===t&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),i&&e.set(s,t))}return t}toString(){return this.cssText}};const o=i?t=>t:t=>t instanceof CSSStyleSheet?(t=>{let i="";for(const s of t.cssRules)i+=s.cssText;return(t=>new r("string"==typeof t?t:t+"",void 0,s))(i)})(t):t
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */;var n;const l=window,h=l.trustedTypes,a=h?h.emptyScript:"",c=l.reactiveElementPolyfillSupport,d={toAttribute(t,i){switch(i){case Boolean:t=t?a:null;break;case Object:case Array:t=null==t?t:JSON.stringify(t)}return t},fromAttribute(t,i){let s=t;switch(i){case Boolean:s=null!==t;break;case Number:s=null===t?null:Number(t);break;case Object:case Array:try{s=JSON.parse(t)}catch(t){s=null}}return s}},u=(t,i)=>i!==t&&(i==i||t==t),v={attribute:!0,type:String,converter:d,reflect:!1,hasChanged:u};let f=class extends HTMLElement{constructor(){super(),this._$Ei=new Map,this.isUpdatePending=!1,this.hasUpdated=!1,this._$El=null,this.u()}static addInitializer(t){var i;this.finalize(),(null!==(i=this.h)&&void 0!==i?i:this.h=[]).push(t)}static get observedAttributes(){this.finalize();const t=[];return this.elementProperties.forEach(((i,s)=>{const e=this._$Ep(s,i);void 0!==e&&(this._$Ev.set(e,s),t.push(e))})),t}static createProperty(t,i=v){if(i.state&&(i.attribute=!1),this.finalize(),this.elementProperties.set(t,i),!i.noAccessor&&!this.prototype.hasOwnProperty(t)){const s="symbol"==typeof t?Symbol():"__"+t,e=this.getPropertyDescriptor(t,s,i);void 0!==e&&Object.defineProperty(this.prototype,t,e)}}static getPropertyDescriptor(t,i,s){return{get(){return this[i]},set(e){const r=this[t];this[i]=e,this.requestUpdate(t,r,s)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)||v}static finalize(){if(this.hasOwnProperty("finalized"))return!1;this.finalized=!0;const t=Object.getPrototypeOf(this);if(t.finalize(),void 0!==t.h&&(this.h=[...t.h]),this.elementProperties=new Map(t.elementProperties),this._$Ev=new Map,this.hasOwnProperty("properties")){const t=this.properties,i=[...Object.getOwnPropertyNames(t),...Object.getOwnPropertySymbols(t)];for(const s of i)this.createProperty(s,t[s])}return this.elementStyles=this.finalizeStyles(this.styles),!0}static finalizeStyles(t){const i=[];if(Array.isArray(t)){const s=new Set(t.flat(1/0).reverse());for(const t of s)i.unshift(o(t))}else void 0!==t&&i.push(o(t));return i}static _$Ep(t,i){const s=i.attribute;return!1===s?void 0:"string"==typeof s?s:"string"==typeof t?t.toLowerCase():void 0}u(){var t;this._$E_=new Promise((t=>this.enableUpdating=t)),this._$AL=new Map,this._$Eg(),this.requestUpdate(),null===(t=this.constructor.h)||void 0===t||t.forEach((t=>t(this)))}addController(t){var i,s;(null!==(i=this._$ES)&&void 0!==i?i:this._$ES=[]).push(t),void 0!==this.renderRoot&&this.isConnected&&(null===(s=t.hostConnected)||void 0===s||s.call(t))}removeController(t){var i;null===(i=this._$ES)||void 0===i||i.splice(this._$ES.indexOf(t)>>>0,1)}_$Eg(){this.constructor.elementProperties.forEach(((t,i)=>{this.hasOwnProperty(i)&&(this._$Ei.set(i,this[i]),delete this[i])}))}createRenderRoot(){var s;const e=null!==(s=this.shadowRoot)&&void 0!==s?s:this.attachShadow(this.constructor.shadowRootOptions);return((s,e)=>{i?s.adoptedStyleSheets=e.map((t=>t instanceof CSSStyleSheet?t:t.styleSheet)):e.forEach((i=>{const e=document.createElement("style"),r=t.litNonce;void 0!==r&&e.setAttribute("nonce",r),e.textContent=i.cssText,s.appendChild(e)}))})(e,this.constructor.elementStyles),e}connectedCallback(){var t;void 0===this.renderRoot&&(this.renderRoot=this.createRenderRoot()),this.enableUpdating(!0),null===(t=this._$ES)||void 0===t||t.forEach((t=>{var i;return null===(i=t.hostConnected)||void 0===i?void 0:i.call(t)}))}enableUpdating(t){}disconnectedCallback(){var t;null===(t=this._$ES)||void 0===t||t.forEach((t=>{var i;return null===(i=t.hostDisconnected)||void 0===i?void 0:i.call(t)}))}attributeChangedCallback(t,i,s){this._$AK(t,s)}_$EO(t,i,s=v){var e;const r=this.constructor._$Ep(t,s);if(void 0!==r&&!0===s.reflect){const o=(void 0!==(null===(e=s.converter)||void 0===e?void 0:e.toAttribute)?s.converter:d).toAttribute(i,s.type);this._$El=t,null==o?this.removeAttribute(r):this.setAttribute(r,o),this._$El=null}}_$AK(t,i){var s;const e=this.constructor,r=e._$Ev.get(t);if(void 0!==r&&this._$El!==r){const t=e.getPropertyOptions(r),o="function"==typeof t.converter?{fromAttribute:t.converter}:void 0!==(null===(s=t.converter)||void 0===s?void 0:s.fromAttribute)?t.converter:d;this._$El=r,this[r]=o.fromAttribute(i,t.type),this._$El=null}}requestUpdate(t,i,s){let e=!0;void 0!==t&&(((s=s||this.constructor.getPropertyOptions(t)).hasChanged||u)(this[t],i)?(this._$AL.has(t)||this._$AL.set(t,i),!0===s.reflect&&this._$El!==t&&(void 0===this._$EC&&(this._$EC=new Map),this._$EC.set(t,s))):e=!1),!this.isUpdatePending&&e&&(this._$E_=this._$Ej())}async _$Ej(){this.isUpdatePending=!0;try{await this._$E_}catch(t){Promise.reject(t)}const t=this.scheduleUpdate();return null!=t&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){var t;if(!this.isUpdatePending)return;this.hasUpdated,this._$Ei&&(this._$Ei.forEach(((t,i)=>this[i]=t)),this._$Ei=void 0);let i=!1;const s=this._$AL;try{i=this.shouldUpdate(s),i?(this.willUpdate(s),null===(t=this._$ES)||void 0===t||t.forEach((t=>{var i;return null===(i=t.hostUpdate)||void 0===i?void 0:i.call(t)})),this.update(s)):this._$Ek()}catch(t){throw i=!1,this._$Ek(),t}i&&this._$AE(s)}willUpdate(t){}_$AE(t){var i;null===(i=this._$ES)||void 0===i||i.forEach((t=>{var i;return null===(i=t.hostUpdated)||void 0===i?void 0:i.call(t)})),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$Ek(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$E_}shouldUpdate(t){return!0}update(t){void 0!==this._$EC&&(this._$EC.forEach(((t,i)=>this._$EO(i,this[i],t))),this._$EC=void 0),this._$Ek()}updated(t){}firstUpdated(t){}};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
var g;f.finalized=!0,f.elementProperties=new Map,f.elementStyles=[],f.shadowRootOptions={mode:"open"},null==c||c({ReactiveElement:f}),(null!==(n=l.reactiveElementVersions)&&void 0!==n?n:l.reactiveElementVersions=[]).push("1.6.1");const m=window,p=m.trustedTypes,b=p?p.createPolicy("lit-html",{createHTML:t=>t}):void 0,w=`lit$${(Math.random()+"").slice(9)}$`,y="?"+w,$=`<${y}>`,k=document,S=(t="")=>k.createComment(t),A=t=>null===t||"object"!=typeof t&&"function"!=typeof t,x=Array.isArray,C=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,M=/-->/g,_=/>/g,z=RegExp(">|[ \t\n\f\r](?:([^\\s\"'>=/]+)([ \t\n\f\r]*=[ \t\n\f\r]*(?:[^ \t\n\f\r\"'`<>=]|(\"|')|))|$)","g"),E=/'/g,L=/"/g,O=/^(?:script|style|textarea|title)$/i,j=(t=>(i,...s)=>({_$litType$:t,strings:i,values:s}))(1),H=Symbol.for("lit-noChange"),T=Symbol.for("lit-nothing"),I=new WeakMap,U=k.createTreeWalker(k,129,null,!1),F=(t,i)=>{const s=t.length-1,e=[];let r,o=2===i?"<svg>":"",n=C;for(let i=0;i<s;i++){const s=t[i];let l,h,a=-1,c=0;for(;c<s.length&&(n.lastIndex=c,h=n.exec(s),null!==h);)c=n.lastIndex,n===C?"!--"===h[1]?n=M:void 0!==h[1]?n=_:void 0!==h[2]?(O.test(h[2])&&(r=RegExp("</"+h[2],"g")),n=z):void 0!==h[3]&&(n=z):n===z?">"===h[0]?(n=null!=r?r:C,a=-1):void 0===h[1]?a=-2:(a=n.lastIndex-h[2].length,l=h[1],n=void 0===h[3]?z:'"'===h[3]?L:E):n===L||n===E?n=z:n===M||n===_?n=C:(n=z,r=void 0);const d=n===z&&t[i+1].startsWith("/>")?" ":"";o+=n===C?s+$:a>=0?(e.push(l),s.slice(0,a)+"$lit$"+s.slice(a)+w+d):s+w+(-2===a?(e.push(void 0),i):d)}const l=o+(t[s]||"<?>")+(2===i?"</svg>":"");if(!Array.isArray(t)||!t.hasOwnProperty("raw"))throw Error("invalid template strings array");return[void 0!==b?b.createHTML(l):l,e]};class B{constructor({strings:t,_$litType$:i},s){let e;this.parts=[];let r=0,o=0;const n=t.length-1,l=this.parts,[h,a]=F(t,i);if(this.el=B.createElement(h,s),U.currentNode=this.el.content,2===i){const t=this.el.content,i=t.firstChild;i.remove(),t.append(...i.childNodes)}for(;null!==(e=U.nextNode())&&l.length<n;){if(1===e.nodeType){if(e.hasAttributes()){const t=[];for(const i of e.getAttributeNames())if(i.endsWith("$lit$")||i.startsWith(w)){const s=a[o++];if(t.push(i),void 0!==s){const t=e.getAttribute(s.toLowerCase()+"$lit$").split(w),i=/([.?@])?(.*)/.exec(s);l.push({type:1,index:r,name:i[2],strings:t,ctor:"."===i[1]?D:"?"===i[1]?J:"@"===i[1]?K:P})}else l.push({type:6,index:r})}for(const i of t)e.removeAttribute(i)}if(O.test(e.tagName)){const t=e.textContent.split(w),i=t.length-1;if(i>0){e.textContent=p?p.emptyScript:"";for(let s=0;s<i;s++)e.append(t[s],S()),U.nextNode(),l.push({type:2,index:++r});e.append(t[i],S())}}}else if(8===e.nodeType)if(e.data===y)l.push({type:2,index:r});else{let t=-1;for(;-1!==(t=e.data.indexOf(w,t+1));)l.push({type:7,index:r}),t+=w.length-1}r++}}static createElement(t,i){const s=k.createElement("template");return s.innerHTML=t,s}}function R(t,i,s=t,e){var r,o,n,l;if(i===H)return i;let h=void 0!==e?null===(r=s._$Co)||void 0===r?void 0:r[e]:s._$Cl;const a=A(i)?void 0:i._$litDirective$;return(null==h?void 0:h.constructor)!==a&&(null===(o=null==h?void 0:h._$AO)||void 0===o||o.call(h,!1),void 0===a?h=void 0:(h=new a(t),h._$AT(t,s,e)),void 0!==e?(null!==(n=(l=s)._$Co)&&void 0!==n?n:l._$Co=[])[e]=h:s._$Cl=h),void 0!==h&&(i=R(t,h._$AS(t,i.values),h,e)),i}class N{constructor(t,i){this.u=[],this._$AN=void 0,this._$AD=t,this._$AM=i}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}v(t){var i;const{el:{content:s},parts:e}=this._$AD,r=(null!==(i=null==t?void 0:t.creationScope)&&void 0!==i?i:k).importNode(s,!0);U.currentNode=r;let o=U.nextNode(),n=0,l=0,h=e[0];for(;void 0!==h;){if(n===h.index){let i;2===h.type?i=new V(o,o.nextSibling,this,t):1===h.type?i=new h.ctor(o,h.name,h.strings,this,t):6===h.type&&(i=new W(o,this,t)),this.u.push(i),h=e[++l]}n!==(null==h?void 0:h.index)&&(o=U.nextNode(),n++)}return r}p(t){let i=0;for(const s of this.u)void 0!==s&&(void 0!==s.strings?(s._$AI(t,s,i),i+=s.strings.length-2):s._$AI(t[i])),i++}}class V{constructor(t,i,s,e){var r;this.type=2,this._$AH=T,this._$AN=void 0,this._$AA=t,this._$AB=i,this._$AM=s,this.options=e,this._$Cm=null===(r=null==e?void 0:e.isConnected)||void 0===r||r}get _$AU(){var t,i;return null!==(i=null===(t=this._$AM)||void 0===t?void 0:t._$AU)&&void 0!==i?i:this._$Cm}get parentNode(){let t=this._$AA.parentNode;const i=this._$AM;return void 0!==i&&11===t.nodeType&&(t=i.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,i=this){t=R(this,t,i),A(t)?t===T||null==t||""===t?(this._$AH!==T&&this._$AR(),this._$AH=T):t!==this._$AH&&t!==H&&this.g(t):void 0!==t._$litType$?this.$(t):void 0!==t.nodeType?this.T(t):(t=>x(t)||"function"==typeof(null==t?void 0:t[Symbol.iterator]))(t)?this.k(t):this.g(t)}O(t,i=this._$AB){return this._$AA.parentNode.insertBefore(t,i)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}g(t){this._$AH!==T&&A(this._$AH)?this._$AA.nextSibling.data=t:this.T(k.createTextNode(t)),this._$AH=t}$(t){var i;const{values:s,_$litType$:e}=t,r="number"==typeof e?this._$AC(t):(void 0===e.el&&(e.el=B.createElement(e.h,this.options)),e);if((null===(i=this._$AH)||void 0===i?void 0:i._$AD)===r)this._$AH.p(s);else{const t=new N(r,this),i=t.v(this.options);t.p(s),this.T(i),this._$AH=t}}_$AC(t){let i=I.get(t.strings);return void 0===i&&I.set(t.strings,i=new B(t)),i}k(t){x(this._$AH)||(this._$AH=[],this._$AR());const i=this._$AH;let s,e=0;for(const r of t)e===i.length?i.push(s=new V(this.O(S()),this.O(S()),this,this.options)):s=i[e],s._$AI(r),e++;e<i.length&&(this._$AR(s&&s._$AB.nextSibling,e),i.length=e)}_$AR(t=this._$AA.nextSibling,i){var s;for(null===(s=this._$AP)||void 0===s||s.call(this,!1,!0,i);t&&t!==this._$AB;){const i=t.nextSibling;t.remove(),t=i}}setConnected(t){var i;void 0===this._$AM&&(this._$Cm=t,null===(i=this._$AP)||void 0===i||i.call(this,t))}}class P{constructor(t,i,s,e,r){this.type=1,this._$AH=T,this._$AN=void 0,this.element=t,this.name=i,this._$AM=e,this.options=r,s.length>2||""!==s[0]||""!==s[1]?(this._$AH=Array(s.length-1).fill(new String),this.strings=s):this._$AH=T}get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}_$AI(t,i=this,s,e){const r=this.strings;let o=!1;if(void 0===r)t=R(this,t,i,0),o=!A(t)||t!==this._$AH&&t!==H,o&&(this._$AH=t);else{const e=t;let n,l;for(t=r[0],n=0;n<r.length-1;n++)l=R(this,e[s+n],i,n),l===H&&(l=this._$AH[n]),o||(o=!A(l)||l!==this._$AH[n]),l===T?t=T:t!==T&&(t+=(null!=l?l:"")+r[n+1]),this._$AH[n]=l}o&&!e&&this.j(t)}j(t){t===T?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,null!=t?t:"")}}class D extends P{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===T?void 0:t}}const Y=p?p.emptyScript:"";class J extends P{constructor(){super(...arguments),this.type=4}j(t){t&&t!==T?this.element.setAttribute(this.name,Y):this.element.removeAttribute(this.name)}}class K extends P{constructor(t,i,s,e,r){super(t,i,s,e,r),this.type=5}_$AI(t,i=this){var s;if((t=null!==(s=R(this,t,i,0))&&void 0!==s?s:T)===H)return;const e=this._$AH,r=t===T&&e!==T||t.capture!==e.capture||t.once!==e.once||t.passive!==e.passive,o=t!==T&&(e===T||r);r&&this.element.removeEventListener(this.name,this,e),o&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){var i,s;"function"==typeof this._$AH?this._$AH.call(null!==(s=null===(i=this.options)||void 0===i?void 0:i.host)&&void 0!==s?s:this.element,t):this._$AH.handleEvent(t)}}class W{constructor(t,i,s){this.element=t,this.type=6,this._$AN=void 0,this._$AM=i,this.options=s}get _$AU(){return this._$AM._$AU}_$AI(t){R(this,t)}}const Z=m.litHtmlPolyfillSupport;null==Z||Z(B,V),(null!==(g=m.litHtmlVersions)&&void 0!==g?g:m.litHtmlVersions=[]).push("2.6.1");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
var q,G;let Q=class extends f{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){var t,i;const s=super.createRenderRoot();return null!==(t=(i=this.renderOptions).renderBefore)&&void 0!==t||(i.renderBefore=s.firstChild),s}update(t){const i=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=((t,i,s)=>{var e,r;const o=null!==(e=null==s?void 0:s.renderBefore)&&void 0!==e?e:i;let n=o._$litPart$;if(void 0===n){const t=null!==(r=null==s?void 0:s.renderBefore)&&void 0!==r?r:null;o._$litPart$=n=new V(i.insertBefore(S(),t),t,void 0,null!=s?s:{})}return n._$AI(t),n})(i,this.renderRoot,this.renderOptions)}connectedCallback(){var t;super.connectedCallback(),null===(t=this._$Do)||void 0===t||t.setConnected(!0)}disconnectedCallback(){var t;super.disconnectedCallback(),null===(t=this._$Do)||void 0===t||t.setConnected(!1)}render(){return H}};Q.finalized=!0,Q._$litElement$=!0,null===(q=globalThis.litElementHydrateSupport)||void 0===q||q.call(globalThis,{LitElement:Q});const X=globalThis.litElementPolyfillSupport;null==X||X({LitElement:Q}),(null!==(G=globalThis.litElementVersions)&&void 0!==G?G:globalThis.litElementVersions=[]).push("3.2.2");
/**
 * @license
 * Copyright 2020 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const tt=2;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */let it=class{constructor(t){}get _$AU(){return this._$AM._$AU}_$AT(t,i,s){this._$Ct=t,this._$AM=i,this._$Ci=s}_$AS(t,i){return this.update(t,i)}update(t,i){return this.render(...i)}};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */const st=(t,i)=>{var s,e;const r=t._$AN;if(void 0===r)return!1;for(const t of r)null===(e=(s=t)._$AO)||void 0===e||e.call(s,i,!1),st(t,i);return!0},et=t=>{let i,s;do{if(void 0===(i=t._$AM))break;s=i._$AN,s.delete(t),t=i}while(0===(null==s?void 0:s.size))},rt=t=>{for(let i;i=t._$AM;t=i){let s=i._$AN;if(void 0===s)i._$AN=s=new Set;else if(s.has(t))break;s.add(t),lt(i)}};function ot(t){void 0!==this._$AN?(et(this),this._$AM=t,rt(this)):this._$AM=t}function nt(t,i=!1,s=0){const e=this._$AH,r=this._$AN;if(void 0!==r&&0!==r.size)if(i)if(Array.isArray(e))for(let t=s;t<e.length;t++)st(e[t],!1),et(e[t]);else null!=e&&(st(e,!1),et(e));else st(this,t)}const lt=t=>{var i,s,e,r;t.type==tt&&(null!==(i=(e=t)._$AP)&&void 0!==i||(e._$AP=nt),null!==(s=(r=t)._$AQ)&&void 0!==s||(r._$AQ=ot))};let ht=class extends it{constructor(){super(...arguments),this._$AN=void 0}_$AT(t,i,s){super._$AT(t,i,s),rt(this),this.isConnected=t._$AU}_$AO(t,i=!0){var s,e;t!==this.isConnected&&(this.isConnected=t,t?null===(s=this.reconnected)||void 0===s||s.call(this):null===(e=this.disconnected)||void 0===e||e.call(this)),i&&(st(this,t),et(this))}setValue(t){if((t=>void 0===t.strings)(this._$Ct))this._$Ct._$AI(t,this);else{const i=[...this._$Ct._$AH];i[this._$Ci]=t,this._$Ct._$AI(i,this,0)}}disconnected(){}reconnected(){}};
/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */class at{constructor(t){this.Y=t}disconnect(){this.Y=void 0}reconnect(t){this.Y=t}deref(){return this.Y}}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const ct=t=>!(t=>null===t||"object"!=typeof t&&"function"!=typeof t)(t)&&"function"==typeof t.then;const dt=(t=>(...i)=>({_$litDirective$:t,values:i}))(class extends ht{constructor(){super(...arguments),this._$Cwt=1073741823,this._$Cyt=[],this._$CK=new at(this),this._$CX=new class{constructor(){this.Z=void 0,this.q=void 0}get(){return this.Z}pause(){var t;null!==(t=this.Z)&&void 0!==t||(this.Z=new Promise((t=>this.q=t)))}resume(){var t;null===(t=this.q)||void 0===t||t.call(this),this.Z=this.q=void 0}}}render(...t){var i;return null!==(i=t.find((t=>!ct(t))))&&void 0!==i?i:H}update(t,i){const s=this._$Cyt;let e=s.length;this._$Cyt=i;const r=this._$CK,o=this._$CX;this.isConnected||this.disconnected();for(let t=0;t<i.length&&!(t>this._$Cwt);t++){const n=i[t];if(!ct(n))return this._$Cwt=t,n;t<e&&n===s[t]||(this._$Cwt=1073741823,e=0,Promise.resolve(n).then((async t=>{for(;o.get();)await o.get();const i=r.deref();if(void 0!==i){const s=i._$Cyt.indexOf(n);s>-1&&s<i._$Cwt&&(i._$Cwt=s,i.setValue(t))}})))}return H}disconnected(){this._$CK.disconnect(),this._$CX.pause()}reconnected(){this._$CK.reconnect(this),this._$CX.resume()}}),ut=(t,i)=>"method"===i.kind&&i.descriptor&&!("value"in i.descriptor)?{...i,finisher(s){s.createProperty(i.key,t)}}:{kind:"field",key:Symbol(),placement:"own",descriptor:{},originalKey:i.key,initializer(){"function"==typeof i.initializer&&(this[i.key]=i.initializer.call(this))},finisher(s){s.createProperty(i.key,t)}};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function vt(t){return(i,s)=>void 0!==s?((t,i,s)=>{i.constructor.createProperty(s,t)})(t,i,s):ut(t,i)
/**
 * @license
 * Copyright 2021 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */}var ft;null===(ft=window.HTMLSlotElement)||void 0===ft||ft.prototype.assignedElements;var gt=function(t,i,s,e){for(var r,o=arguments.length,n=o<3?i:null===e?e=Object.getOwnPropertyDescriptor(i,s):e,l=t.length-1;l>=0;l--)(r=t[l])&&(n=(o<3?r(n):o>3?r(i,s,n):r(i,s))||n);return o>3&&n&&Object.defineProperty(i,s,n),n};let mt=class extends Q{constructor(){super(...arguments),this.skillset={name:"Loading...",description:"Loading...",skillcount:0,skills:[]},this.entries=[],this.experts=[],this.showSkillStats=!1,this.showMemberStats=!1}connectedCallback(){super.connectedCallback(),console.log(this.entries),console.log(this.skillset),this.activeMembers=[...new Set(this.entries.map((t=>t.user)))].length,this.averagemembercompletionpercentage=Math.round(this.entries.length/(this.skillset.skills.length*this.skillset.brand.memberCount)*1e4)/100,this.averageactivemembercompletionpercentage=Math.round(this.entries.length/(this.skillset.skills.length*this.activeMembers)*1e4)/100,this.experts=Array.from(this.groupBy(this.entries,(t=>t.user))).filter((t=>t[1].length==this.skillset.skills.length))}SkillSetStatistics(){return j`
        <article class="skill-card-header">
          <header>
            <img class="skillset-logo" src="https://www.skilldisplay.eu/${this.skillset.mediaPublicUrl}" />             
            <div class="skillset-info">
              <div>
                <h3 class="skillset-header">${this.skillset.name}</h3>
                <div class="achievement-count">                    
                    ${this.skillset.skills.length} Achievements available
                </div>               
              </div>              
                <div class="skillset-progress organisation-badge">
                    <img src="https://www.skilldisplay.eu/${this.skillset.brand.logoPublicUrl}" />
                </div>
            </div>            
          </header>
          <div class="skill-card">            
              <article class="rating-organisation">              
                  <div class="rating-organisation-stars">${this.renderStarSVG(Math.floor(this.averageactivemembercompletionpercentage/20)+1)}</div>
                  <h4>Organisation Rating</h4>
                  <table>
                      <tr>
                          <td>Active Members</td>
                          <td class="rating-value">${this.activeMembers} of ${this.skillset.brand.memberCount}</td>
                      </tr>
                      <tr>
                          <td>Badges Claimed</td>
                          <td class="rating-value">${this.experts.length}</td>
                      </tr>
                      <tr>
                          <td>Average Total Member Completion</td>
                          <td class="rating-value">${this.averagemembercompletionpercentage} %</td>
                      </tr>
                      <tr>
                          <td>Average Active Member Completion</td>
                          <td class="rating-value">${this.averageactivemembercompletionpercentage} %</td>
                      </tr>
                  </table>                  
              </article>
              <div class="tabcontrol">
                  <a class="tabcontrol-item" @click="${this._switchSkillstatsVisibility}">${this.renderListIcon()}</a>
                  <a class="tabcontrol-item" @click="${this._switchMemberstatsVisibility}">${this.renderUserIcon()}</a>
              </div>
              ${this.renderSkillStatsBlock()}
              ${this.renderMemberStatsBlock()}
          </div>         
          <div class="skill-card-footer">
            powered by <img src="https://www.skilldisplay.eu/typo3conf/ext/skilldisplay/Resources/Public/Images/SkillDisplay_Logo_Text_Only.svg">
          </div>
        </article>`}_switchMemberstatsVisibility(){this.showSkillStats=!1,this.showMemberStats=!this.showMemberStats,this.requestUpdate()}_switchSkillstatsVisibility(){this.showMemberStats=!1,this.showSkillStats=!this.showSkillStats,this.requestUpdate()}renderSkillStatsBlock(){return this.showSkillStats?j`
                <article class="rating-organisation">
                    <h4>Achievement Claims</h4>
                    <table>
                        ${Array.from(this.groupBy(this.entries,(t=>t.skill))).sort(((t,i)=>i[1].length-t[1].length)).map((t=>j`<tr>
                                <td>${t[0]}</td>
                                <td class="rating-value">${t[1].length}</td>
                            </tr>`))}
                    </table>
                </article>
            `:j``}renderMemberStatsBlock(){return this.showMemberStats?j`
          <article class="rating-organisation">
              <h4>Badge claimed by</h4>
                <table>
                ${this.experts.map((t=>j`<tr>
                        <td>${t[1][0].firstName}</td>
                        <!-- <td class="rating-value">${t[1].length}</td> -->
                    </tr>`))}
                </table>
                </article>`:j``}groupBy(t,i){const s=new Map;return t.forEach((t=>{const e=i(t),r=s.get(e);r?r.push(t):s.set(e,[t])})),s}renderStarSVG(t){const i=[];for(let s=0;s<t;s++)i.push(j`<svg width="20" height="20" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 194.22 184.73">
      <path fill="#fc0" d="m157.09624313 184.71807481-60.01047609-31.56494446-60.02437133 31.54175224 11.4755592-66.82748987L-.00931154 70.52751534l67.1029075-9.73708948L97.11501258-.00816438l29.9970934 60.81131153 67.0986995 9.76367105-48.56377264 47.3191202z"/>
      <path fill="#ffe680" d="M127.315 60.416c-30.39 41.369-30.72 41.819-30.72 41.819l97.411-31.899z"/>
      <path fill="#fd5" d="M97.095 101.346v51.942l-60.63 31.117zm0 0 59.613 81.476-11.189-65.984z"/>
      <path fill="#ffe680" d="M.385 70.406 97.1 101.348 67.218 60.506z"/>
      <path fill="#fd5" d="M97.095 101.346V.126l29.83 60.357z"/>
      <path fill="#ffd42a" d="m37.085 183.566 11.261-66.541 48.757-15.679z"/>
    </svg>`);return i}renderUserIcon(){return j`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512"><!--! Font Awesome Free 6.3.0 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License) Copyright 2023 Fonticons, Inc. --><path d="M304 128a80 80 0 1 0 -160 0 80 80 0 1 0 160 0zM96 128a128 128 0 1 1 256 0A128 128 0 1 1 96 128zM49.3 464H398.7c-8.9-63.3-63.3-112-129-112H178.3c-65.7 0-120.1 48.7-129 112zM0 482.3C0 383.8 79.8 304 178.3 304h91.4C368.2 304 448 383.8 448 482.3c0 16.4-13.3 29.7-29.7 29.7H29.7C13.3 512 0 498.7 0 482.3z"/></svg>`}renderListIcon(){return j`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><!--! Font Awesome Free 6.3.0 by @fontawesome - https://fontawesome.com License - https://fontawesome.com/license/free (Icons: CC BY 4.0, Fonts: SIL OFL 1.1, Code: MIT License) Copyright 2023 Fonticons, Inc. --><path d="M40 48C26.7 48 16 58.7 16 72v48c0 13.3 10.7 24 24 24H88c13.3 0 24-10.7 24-24V72c0-13.3-10.7-24-24-24H40zM192 64c-17.7 0-32 14.3-32 32s14.3 32 32 32H480c17.7 0 32-14.3 32-32s-14.3-32-32-32H192zm0 160c-17.7 0-32 14.3-32 32s14.3 32 32 32H480c17.7 0 32-14.3 32-32s-14.3-32-32-32H192zm0 160c-17.7 0-32 14.3-32 32s14.3 32 32 32H480c17.7 0 32-14.3 32-32s-14.3-32-32-32H192zM16 232v48c0 13.3 10.7 24 24 24H88c13.3 0 24-10.7 24-24V232c0-13.3-10.7-24-24-24H40c-13.3 0-24 10.7-24 24zM40 368c-13.3 0-24 10.7-24 24v48c0 13.3 10.7 24 24 24H88c13.3 0 24-10.7 24-24V392c0-13.3-10.7-24-24-24H40z"/></svg>`}render(){return j`
      ${dt(j`
            ${this.SkillSetStatistics()}
          `,j`
            <span>Loading...</span>
          `)}
    `}};mt.styles=((t,...i)=>{const e=1===t.length?t[0]:i.reduce(((i,s,e)=>i+(t=>{if(!0===t._$cssResult$)return t.cssText;if("number"==typeof t)return t;throw Error("Value passed to 'css' function must be a 'css' function result: "+t+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(s)+t[e+1]),t[0]);return new r(e,t,s)})`
        * {
          font-family: Lato, sans-serif;
        }

        .skillset-logo{
          background-color: #e0e0e0;
          object-fit: cover;
          width: 100%;
          min-height: 7rem;
          border-radius: .35rem .35rem 0 0;
          max-height: 7rem;
          display: block;
        }
        
        .skill-card {
          background-color: #f8f8f8;
          padding: 0.8rem;
          border-radius: .35rem;
        }

        .skill-card-header {
          display: flex;
          flex-direction: column;
          justify-content: center;
          background-color: #fff;
          border: .05rem solid #E0E0E0;
          border-radius: .35rem;
          margin: 0.2rem auto;
          max-width: 60rem;
        }
              
        div.skillset-info{
          display: flex;
          align-items: start;
          background-color: #FFF;
          padding: 0 0.6rem;
        }
        
        h3.skillset-header{
          margin: 0.6rem 0 0 0;
        }

        h4{
          margin: 0.4rem 0;
        }
        
        .achievement-count{          
          font-size: 0.9rem;
          color: #828282;
          margin: 0 0 0.8rem 0;
        }

        .skill-card-footer {
          display: flex;
          flex-direction: row;
          justify-content: center;
          align-content: center;
          padding: 0.5rem 0 0 0;
          font-size: 8pt;
        }

        .skill-card-footer img {
          max-height: 2rem;
          translate: -0.3rem -0.7rem;
        }
        
        .organisation-badge{
          margin-left: auto;
          margin-right: 0.1rem;
          padding: 0.2rem;
          font-size: 1.2em;
          white-space: nowrap;  
          height: 3rem;
            width: auto;
            min-width: 6rem;
            overflow: hidden;
            border: .05rem solid #E0E0E0;
            border-radius: 1.5rem;
            border-color: #fff;
            background-color: #fff;
            box-shadow: 0 .1rem .2rem #00000040;           
            transition: all .2s;
          transform: translateY(-50%);
        }

        .organisation-badge img{
          height: 100%;
        }
        
        article.rating-organisation{
          margin-bottom: 1rem;
        }
        
        article.rating-organisation table{
          width: 100%;
        }
        
        div.rating-organisation-stars{
          float:right;
          transform: translateY(-0.2rem);
        }
        
        td.rating-value{
          text-align:right;
          vertical-align: top;
          min-width: 3rem;
        }
        
        div.tabcontrol{
          width: 100%;
          text-align: right;
        }
        
        a.tabcontrol-item svg{
          width: 1rem;
          height: 1rem;
          border: 1px black solid;
          padding: 0.2rem;
        }
        
      `,gt([vt({type:Object})],mt.prototype,"skillset",void 0),gt([vt({type:Array})],mt.prototype,"entries",void 0),gt([vt()],mt.prototype,"activeMembers",void 0),gt([vt()],mt.prototype,"averagemembercompletionpercentage",void 0),gt([vt()],mt.prototype,"averageactivemembercompletionpercentage",void 0),gt([vt()],mt.prototype,"experts",void 0),gt([vt()],mt.prototype,"showSkillStats",void 0),gt([vt()],mt.prototype,"showMemberStats",void 0),mt=gt([(t=>i=>"function"==typeof i?((t,i)=>(customElements.define(t,i),i))(t,i):((t,i)=>{const{kind:s,elements:e}=i;return{kind:s,elements:e,finisher(i){customElements.define(t,i)}}})(t,i)
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */)("skillset-stats")],mt);export{mt as SkillSetStats};
