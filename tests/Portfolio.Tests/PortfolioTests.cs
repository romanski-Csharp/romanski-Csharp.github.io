using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Portfolio.Tests;

[Parallelizable(ParallelScope.Self)]
public class PortfolioTests : PageTest
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:8080";

    [SetUp]
    public async Task OpenHome() => await Page.GotoAsync(BaseUrl);

    [Test]
    public async Task HomePage_HasCorrectTitleAndHero()
    {
        await Expect(Page).ToHaveTitleAsync(new Regex("Roman Chub"));
        await Expect(Page.GetByTestId("hero-title")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Projects_AllCardsAreRendered()
    {
        await Expect(Page.GetByTestId("project-card")).ToHaveCountAsync(3);
    }

    [TestCase("About", "#about")]
    [TestCase("Projects", "#projects")]
    [TestCase("Contact", "#contact")]
    public async Task Navigation_LinkScrollsToSection(string link, string hash)
    {
        await Page.GetByTestId("navbar").GetByRole(AriaRole.Link, new() { Name = link }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(hash + "$"));
    }

    [Test]
    public async Task ThemeToggle_SwitchesAndPersistsTheme()
    {
        var html = Page.Locator("html");
        await Expect(html).ToHaveAttributeAsync("data-theme", "dark");

        await Page.GetByTestId("theme-toggle").ClickAsync();
        await Expect(html).ToHaveAttributeAsync("data-theme", "light");

        await Page.ReloadAsync();
        await Expect(html).ToHaveAttributeAsync("data-theme", "light");
    }

    [Test]
    public async Task CodeTabs_SwitchVisiblePanel()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login.feature" }).ClickAsync();
        await Expect(Page.Locator("#bdd")).ToBeVisibleAsync();
        await Expect(Page.Locator("#pom")).ToBeHiddenAsync();
    }

    [Test]
    public async Task Mobile_BurgerMenuOpensNavigation()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await Page.GetByTestId("burger").ClickAsync();
        await Expect(Page.Locator("#navLinks")).ToHaveClassAsync(new Regex("open"));
    }

    [Test]
    public async Task CvDownload_FileIsAvailable()
    {
        var href = await Page.GetByTestId("cta-cv").GetAttributeAsync("href");
        var response = await Page.APIRequest.GetAsync($"{BaseUrl}/{href}");
        Assert.That(response.Ok, Is.True, $"CV file '{href}' is not reachable");
    }
}