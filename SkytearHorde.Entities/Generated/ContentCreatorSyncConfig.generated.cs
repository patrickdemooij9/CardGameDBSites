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
	// Mixin Content Type with alias "contentCreatorSyncConfig"
	/// <summary>Content Creator Sync Config</summary>
	public partial interface IContentCreatorSyncConfig : IPublishedElement
	{
	}

	/// <summary>Content Creator Sync Config</summary>
	[PublishedModel("contentCreatorSyncConfig")]
	public partial class ContentCreatorSyncConfig : PublishedElementModel, IContentCreatorSyncConfig
	{
		// helpers
#pragma warning disable 0109 // new is redundant
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		public new const string ModelTypeAlias = "contentCreatorSyncConfig";
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		public new const PublishedItemType ModelItemType = PublishedItemType.Content;
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public new static IPublishedContentType GetModelContentType(IPublishedSnapshotAccessor publishedSnapshotAccessor)
			=> PublishedModelUtility.GetModelContentType(publishedSnapshotAccessor, ModelItemType, ModelTypeAlias);
		[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Umbraco.ModelsBuilder.Embedded", "13.3.0+a325ba3")]
		[return: global::System.Diagnostics.CodeAnalysis.MaybeNull]
		public static IPublishedPropertyType GetModelPropertyType<TValue>(IPublishedSnapshotAccessor publishedSnapshotAccessor, Expression<Func<ContentCreatorSyncConfig, TValue>> selector)
			=> PublishedModelUtility.GetModelPropertyType(GetModelContentType(publishedSnapshotAccessor), selector);
#pragma warning restore 0109

		private IPublishedValueFallback _publishedValueFallback;

		// ctor
		public ContentCreatorSyncConfig(IPublishedElement content, IPublishedValueFallback publishedValueFallback)
			: base(content, publishedValueFallback)
		{
			_publishedValueFallback = publishedValueFallback;
		}

		// properties
	}
}
