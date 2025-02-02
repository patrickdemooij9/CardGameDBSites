//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a tool.
//
//    Umbraco.ModelsBuilder.Embedded v13.3.0+a325ba3
//
//   Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.ModelsBuilder;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace SkytearHorde.Entities.Generated
{
	/// <summary>Squad Settings</summary>
	[PublishedModel("squadSettings")]
	public partial class SquadSettings : PublishedContentModel
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		public new const string ModelTypeAlias = "squadSettings";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<SquadSettings, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public SquadSettings(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties

		///<summary>
		/// Amount of squad cards
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[ImplementPropertyType("amountOfSquadCards")]
		public virtual int AmountOfSquadCards => this.Value<int>(_publishedValueFallback, "amountOfSquadCards");

		///<summary>
		/// Color Logic
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("colorLogic")]
		public virtual global::Umbraco.Cms.Core.Models.Blocks.BlockListModel ColorLogic => this.Value<global::Umbraco.Cms.Core.Models.Blocks.BlockListModel>(_publishedValueFallback, "colorLogic");

		///<summary>
		/// Default Names
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("defaultNames")]
		public virtual global::System.Collections.Generic.IEnumerable<string> DefaultNames => this.Value<global::System.Collections.Generic.IEnumerable<string>>(_publishedValueFallback, "defaultNames");

		///<summary>
		/// Main Card
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("mainCard")]
		public virtual global::Umbraco.Cms.Core.Models.Blocks.BlockListModel MainCard => this.Value<global::Umbraco.Cms.Core.Models.Blocks.BlockListModel>(_publishedValueFallback, "mainCard");

		///<summary>
		/// Max Dynamic Slots
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[ImplementPropertyType("maxDynamicSlots")]
		public virtual int MaxDynamicSlots => this.Value<int>(_publishedValueFallback, "maxDynamicSlots");

		///<summary>
		/// Overwrite Amount: If filled in, this overwrites the amounts for all cards
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[ImplementPropertyType("overwriteAmount")]
		public virtual int OverwriteAmount => this.Value<int>(_publishedValueFallback, "overwriteAmount");

		///<summary>
		/// Restrictions: Restrictions for all the cards together
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("restrictions")]
		public virtual global::Umbraco.Cms.Core.Models.Blocks.BlockListModel Restrictions => this.Value<global::Umbraco.Cms.Core.Models.Blocks.BlockListModel>(_publishedValueFallback, "restrictions");

		///<summary>
		/// Show Deck Colors
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[ImplementPropertyType("showDeckColors")]
		public virtual bool ShowDeckColors => this.Value<bool>(_publishedValueFallback, "showDeckColors");

		///<summary>
		/// Slot only mode
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[ImplementPropertyType("slotOnlyMode")]
		public virtual bool SlotOnlyMode => this.Value<bool>(_publishedValueFallback, "slotOnlyMode");

		///<summary>
		/// Special Image Rule: Special rule for the icon image shown
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("specialImageRule")]
		public virtual global::Umbraco.Cms.Core.Models.Blocks.BlockListModel SpecialImageRule => this.Value<global::Umbraco.Cms.Core.Models.Blocks.BlockListModel>(_publishedValueFallback, "specialImageRule");

		///<summary>
		/// Squads
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("squads")]
		public virtual global::Umbraco.Cms.Core.Models.Blocks.BlockListModel Squads => this.Value<global::Umbraco.Cms.Core.Models.Blocks.BlockListModel>(_publishedValueFallback, "squads");

		///<summary>
		/// Type Display Name
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[global::System.Diagnostics.CodeAnalysis.MaybeNull]
		[ImplementPropertyType("typeDisplayName")]
		public virtual string TypeDisplayName => this.Value<string>(_publishedValueFallback, "typeDisplayName");

		///<summary>
		/// Type ID
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[ImplementPropertyType("typeID")]
		public virtual int TypeID => this.Value<int>(_publishedValueFallback, "typeID");

		///<summary>
		/// Use Compact Deck Display
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[ImplementPropertyType("useCompactDeckDisplay")]
		public virtual bool UseCompactDeckDisplay => this.Value<bool>(_publishedValueFallback, "useCompactDeckDisplay");

		///<summary>
		/// Use Deck Display
		///</summary>
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[ImplementPropertyType("useDeckDisplay")]
		public virtual bool UseDeckDisplay => this.Value<bool>(_publishedValueFallback, "useDeckDisplay");
	}
}
