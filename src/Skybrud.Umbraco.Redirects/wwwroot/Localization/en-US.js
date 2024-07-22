﻿export default {
	redirects: {
		save: "Save",
		addRedirect: "Add redirect",
		reload: "Reload",
		enabled: "Enabled",
		disabled: "Disabled",
		temporary: "Temporary",
		permanent: "Permanent",
		content: "Content",
		media: "Media",
		url: "URL",
		originalUrl: "Original URL",
		type: "Type",
		destination: "Destination",
		site: "Site",
		allSites: "All sites",
		globalRedirects: "Global redirects",
		allTypes: "All types"
	},
	redirectsTabs: {
		settings: "Settings",
		info: "Info"
	},
	redirectsLabels: {
		advancedOptions: "Advanced Options",
		noRedirects: "There has not yet been added any redirects.",
		noSearchRedirects: "Your search did not match any redirects."
	},
	redirectsProperties: {
		site: "Site",
		siteDescription: "Select the site (or root node) the redirect should apply to. If a site is not selected, the redirect will apply to all domains/sites in the Umbraco solution.",
		originalUrl: "Original URL",
		originalUrlDescription: "Specify the original URL to match from which the user should be redirected to the destination.",
		destination: "Destination",
		destinationDescription: "Select the page or URL the user should be redirected to.",
		originalUrlDescription: "Specify the original URL to match from which the user should be redirected to the destination.",
		destinationCulture: "Culture",
		destinationCultureDescription: "Select the culture of the destination.",
		redirectType: "Redirect type",
		redirectTypeDescription: "Select the type of the redirect. Notice that browsers will remember permanent redirects.",
		forwardQueryString: "Forward query string",
		forwardQueryStringDescription: "When enabled, the query string of the original request is forwarded to the redirect location (pass through).",
		id: "ID",
		key: "Key",
		createDate: "Created",
		updateDate: "Last updated"
	},
	redirectsErrors: {
		redirectAlreadyExists: "A redirect with the same URL and query string already exists."
	}
}