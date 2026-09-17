# UI — sleek Lumina business

`xmlns:nv="http://nuvyntralabs.com/uikit"`. Standing law: `.nuvyn/reference/constraints.md`.

Ship a **modern business** UI for **this** product on Android, iOS, Windows, and Mac Catalyst. Do not assume a clinic or any other vertical. One density, Outfit via UIKit, aurora tokens, short verbs, always empty / error / busy. Phone-first layouts that still read on tablet/desktop. No raw `Entry`/`Button`/`Label` when an `NV*` exists. No one-off hex colors.

**Compose.** Recipes seed demo children and have no Email / Password / Command. One recipe per screen. Put bound primitives **inside** so they replace the seed. Samples stay here — do not recopy them into spec/plan.

### Form

```xml
<nv:NVSignInView>
    <VerticalStackLayout Padding="20" Spacing="16">
        <nv:NVInputField Label="Email" Text="{Binding Email}" />
        <nv:NVPasswordField Label="Password" Text="{Binding Password}" />
        <nv:NVButton Text="Sign in" Variant="Filled" Command="{Binding SignInCommand}" />
    </VerticalStackLayout>
</nv:NVSignInView>
```

### List

```xml
<nv:NVCatalogView>
    <nv:NVCollectionView Items="{Binding Items}" LayoutMode="List" />
</nv:NVCatalogView>
```

### Empty / busy

```xml
<Grid>
    <nv:NVCollectionView Items="{Binding Items}" IsVisible="{Binding HasItems}" />
    <nv:NVEmptyView Title="Nothing yet" Reason="Empty" IsVisible="{Binding IsEmpty}" />
    <nv:NVBusyIndicator IsVisible="{Binding IsBusy}" />
</Grid>
```

Recipes: `.nuvyn/reference/screen-recipes.md`. New screen steps: `.nuvyn/reference/implement-recipes.md`. Packages and data: `constraints.md` (catalog first, API until asked to persist).
