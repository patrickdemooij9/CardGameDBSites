import Alpine from 'alpinejs'
import filteredOverview from './filteredOverview';
import copyToClipboardButton from "./copyToClipboardButton";
import modalButton from './modalButton';
import classHover from './classHover';
import ajaxForm from './ajaxForm';
import tooltip from "./tooltip";
import serverSideOverview from './serverSideOverview';
import cursorImage from './cursorImage';
import importModal from './components/importModal';
import toggler from './utils/toggler';
import anchorHover from './anchorHover';
import listManager from './components/listManager';
 
(window as any).Alpine = Alpine;

Alpine.data('filteredOverview', filteredOverview);
Alpine.data('serverSideOverview', serverSideOverview);
Alpine.data("importModal", importModal);

Alpine.start()

copyToClipboardButton();
modalButton();
classHover();
ajaxForm();
tooltip();
cursorImage();
toggler();
anchorHover();
listManager();