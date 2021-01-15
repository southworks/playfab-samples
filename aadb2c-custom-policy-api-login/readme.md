# Integrate PlayFab with Azure AD B2C using custom policies

## Index

- [Summary][summary]
  - [Prerequisites][prerequisites]
- [How this works][how-this-works]
- [Highlights][highlights]
  - [How custom attributes are managed][how-custom-attributes-are-managed]
  - [How appsettings works][how-appsettings-works]

## Summary

Here we demonstrate how to integrate PlayFab's Users with Azure AD B2C using custom policies.

This sample includes the custom policies necessary for this integration, and an Azure Function that could work as an example of how to make Azure AD B2C and PlayFab interact.

### Prerequisites

If you are new to Azure AD B2C or custom policies, we recommend reading the following Microsoft documentation:

- [What is Azure Active Directory B2C?][azure-b2c-overview]
- [Custom policies in Azure Active Directory B2C][custom-policies-overview]
- [Get started with custom policies in Azure Active Directory B2C][custom-policies-get-started].

## How this works

This diagram explains the flow of the Sign In / Sign Up custom policy.

---

![SUSI flow diagram][susi-flow-diagram]

---

## Highlights

### How custom attributes are managed

Microsoft has a [document][custom-attributes-def-doc] that could help you understand how custom attributes work on custom policies.

We have defined our custom attributes on the [TrustFrameworkExtensions.xml][custom-attributes-def] file.

These are updated on the [PlayFabSUSI technical profile][playfabsusi-technical-profile] by defining them as `OutputClaims`. This indicates that the attributes should be updated with the values returned by the API that has been triggered.

### How appsettings works

The [appsettings.json][appsettings-file] is a configuration file that can be used to build custom policies with dynamic values.

To make this work, you should use the [Azure AD B2C Visual Studio Code extension][aadb2c-vsc-extension].

Once installed, you can build them by pressing `CTRL + SHIFT + 5`.

<!-- Index -->
[summary]: #summary
[prerequisites]: #prerequisites
[how-this-works]: #how-this-works
[highlights]: #highlights
[how-custom-attributes-are-managed]: #how-custom-attributes-are-managed
[how-appsettings-works]: #how-appsettings-works

<!-- Images -->
[susi-flow-diagram]: ./document-assets/susi-flow-diagram.png

<!-- External documents -->
[azure-b2c-overview]: https://docs.microsoft.com/azure/active-directory-b2c/overview
[custom-policies-overview]: https://docs.microsoft.com/azure/active-directory-b2c/custom-policy-overview
[custom-policies-get-started]: https://docs.microsoft.com/azure/active-directory-b2c/custom-policy-get-started
[custom-attributes-def-doc]: https://docs.microsoft.com/azure/active-directory-b2c/custom-policy-custom-attributes
[aadb2c-vsc-extension]: https://marketplace.visualstudio.com/items?itemName=AzureADB2CTools.aadb2c

<!-- Internal references -->
[custom-attributes-def]: ./custom-policies/TrustFrameworkExtensions.xml#L18
[playfabsusi-technical-profile]: ./custom-policies/TrustFrameworkExtensions.xml#L50
[appsettings-file]: ./custom-policies/appsettings.json