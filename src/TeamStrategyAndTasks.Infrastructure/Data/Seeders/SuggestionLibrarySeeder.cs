using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities.Suggestions;

namespace TeamStrategyAndTasks.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds the read-only suggestion library hierarchy that guides users in building their strategy.
/// Call SeedAsync once at application startup (idempotent — skips if data already exists).
/// </summary>
public static class SuggestionLibrarySeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.SuggestionObjectives.AnyAsync()) return;

        // ── Objectives ──────────────────────────────────────────────
        var objGrow    = Obj("Accelerate Revenue Growth",         "Grow top-line revenue through new markets, products, and customers.");
        var objOps     = Obj("Improve Operational Efficiency",    "Reduce cost and waste while maintaining quality.");
        var objTalent  = Obj("Build a High-Performance Culture",  "Attract, develop, and retain top talent.");

        // ── Processes ────────────────────────────────────────────────
        var pSales     = Proc("Sales & Business Development",   "Identify and close new business opportunities.");
        var pMarketing = Proc("Marketing & Demand Generation",  "Build brand awareness and fill the sales pipeline.");
        var pProduct   = Proc("Product Development",            "Design and ship competitive products and features.");
        var pCS        = Proc("Customer Success",               "Onboard, retain, and grow existing customer relationships.");

        var pProcure   = Proc("Procurement & Vendor Management", "Optimise supplier relationships and purchasing costs.");
        var pProcess   = Proc("Process Improvement",            "Continuously improve internal workflows and eliminate waste.");
        var pIT        = Proc("IT & Systems Management",        "Maintain resilient, secure, and efficient technology infrastructure.");
        var pFinance   = Proc("Finance & Cost Management",      "Drive financial discipline and cost visibility.");

        var pRecruit   = Proc("Talent Acquisition",             "Attract and hire the right people quickly.");
        var pLD        = Proc("Learning & Development",         "Upskill employees and build organisational capability.");
        var pEngage    = Proc("Employee Engagement & Retention","Create conditions for people to thrive and stay.");
        var pPerf      = Proc("Performance Management",         "Set clear expectations and hold people accountable.");

        // ── Initiatives ──────────────────────────────────────────────

        // Sales initiatives
        var iSalesCRM  = Init("Implement CRM system",                      "Deploy and customise a CRM to track pipelines and activities.");
        var iSalesBDR  = Init("Launch outbound BDR programme",             "Build a dedicated outbound Business Development Rep team.");
        var iSalesPartn= Init("Establish channel partner programme",       "Recruit and enable reseller and referral partners.");

        // Marketing initiatives
        var iSEO       = Init("Launch SEO & content marketing programme",  "Produce high-value content to drive inbound leads.");
        var iPPC       = Init("Run targeted paid acquisition campaigns",   "Manage PPC and social ad spend for pipeline generation.");
        var iEvents    = Init("Organise thought-leadership events",        "Host webinars and in-person events to build brand authority.");

        // Product initiatives
        var iProdRoadmap = Init("Define and publish product roadmap",      "Align stakeholders on a 12-month feature roadmap.");
        var iUXResearch  = Init("Conduct user-research programme",         "Run regular user interviews and usability tests to inform design.");
        var iShipping    = Init("Implement continuous delivery pipeline",  "Enable teams to ship to production multiple times per day.");

        // Customer Success initiatives
        var iOnboard   = Init("Build customer onboarding playbook",        "Standardise the onboarding journey to reduce time-to-value.");
        var iCSQBR     = Init("Run quarterly business reviews (QBRs)",     "Deliver structured QBRs to demonstrate value and expand accounts.");
        var iNPS       = Init("Launch NPS & feedback programme",           "Systematically collect and act on customer feedback.");

        // Procurement initiatives
        var iProcAudit = Init("Conduct supplier cost audit",               "Review all supplier contracts and identify savings opportunities.");
        var iProcSRM   = Init("Implement supplier relationship management","Build a structured SRM process for top-tier vendors.");

        // Process improvement initiatives
        var iLean      = Init("Run Lean process improvement workshops",    "Map and eliminate waste in the top five core processes.");
        var iAutomate  = Init("Automate repetitive manual workflows",      "Use RPA or scripting to remove human effort from routine tasks.");

        // IT initiatives
        var iCloud     = Init("Migrate workloads to cloud",                "Move on-premises servers to a managed cloud provider.");
        var iSecurity  = Init("Implement information security programme",  "Achieve ISO 27001 or SOC 2 compliance.");

        // Finance initiatives
        var iBudget    = Init("Implement zero-based budgeting",            "Rebuild the annual budget from zero to challenge cost assumptions.");
        var iReporting = Init("Deploy real-time financial dashboards",     "Give budget owners self-serve visibility into spend vs budget.");

        // Talent acquisition initiatives
        var iEVP       = Init("Define employee value proposition (EVP)",   "Articulate why top talent should join and stay.");
        var iATS       = Init("Deploy applicant tracking system (ATS)",    "Implement ATS to structure and accelerate hiring.");
        var iReferral  = Init("Launch employee referral programme",        "Incentivise employees to refer high-quality candidates.");

        // L&D initiatives
        var iLMS       = Init("Deploy learning management system (LMS)",   "Provide self-serve e-learning for all employees.");
        var iMentor    = Init("Create mentoring & coaching programme",     "Pair high-potentials with senior leaders for development.");

        // Engagement & retention initiatives
        var iPulse     = Init("Run regular employee pulse surveys",        "Collect frequent feedback to detect and address engagement issues.");
        var iWellbeing = Init("Launch employee wellbeing programme",       "Provide mental health resources and flexible working options.");

        // Performance management initiatives
        var iOKR       = Init("Roll out OKR framework",                    "Align company and team goals using Objectives & Key Results.");
        var i1on1      = Init("Standardise manager 1-on-1s",              "Equip managers with a structured 1-on-1 agenda and cadence.");

        // ── Tasks ─────────────────────────────────────────────────────

        // CRM tasks
        var tCRMReq    = Task("Document CRM requirements",           "Gather requirements from Sales, CS, and Marketing stakeholders.");
        var tCRMEval   = Task("Evaluate and select CRM vendor",      "Score at least 3 vendors against requirements and budget.");
        var tCRMData   = Task("Migrate existing customer data",      "Clean and import existing contacts, accounts and deals.");
        var tCRMTrain  = Task("Train sales team on CRM usage",       "Deliver hands-on training and create a usage guide.");

        // BDR programme tasks
        var tBDRHire   = Task("Hire initial BDR cohort",             "Recruit 2–4 BDRs with relevant outbound experience.");
        var tBDRPlaybook = Task("Create BDR outreach playbook",      "Document messaging, sequences, and qualification criteria.");
        var tBDRMetrics  = Task("Define BDR KPIs and targets",       "Set activity, pipeline, and conversion targets for the BDR team.");

        // SEO tasks
        var tSEOAudit  = Task("Conduct SEO site audit",              "Identify technical and on-page SEO issues across the site.");
        var tSEOContent = Task("Build content calendar (12 months)", "Plan pillar and cluster content aligned to target keywords.");
        var tSEOLinks  = Task("Execute link-building outreach",      "Secure high-authority backlinks through guest posts and PR.");

        // Product roadmap tasks
        var tRoadStake = Task("Facilitate roadmap stakeholder workshops", "Run structured sessions to align on priorities with business stakeholders.");
        var tRoadPublish = Task("Publish roadmap to company wiki",   "Format and share the roadmap so all teams have visibility.");

        // UX research tasks
        var tUXRecruit = Task("Recruit user research participants",  "Set up a panel of target users for ongoing research.");
        var tUXInterviews = Task("Conduct monthly user interviews",  "Run 5–8 user interviews per month and synthesise findings.");

        // Onboarding playbook tasks
        var tOnboard1  = Task("Map current onboarding journey",      "Document every step a new customer takes in their first 90 days.");
        var tOnboard2  = Task("Create onboarding email sequences",   "Write automated email sequences for each onboarding milestone.");
        var tOnboard3  = Task("Build onboarding success scorecard",  "Define metrics to measure onboarding health per customer.");

        // Supplier audit tasks
        var tAudit1    = Task("Catalogue all active supplier contracts", "Collect and centralise all current supplier agreements.");
        var tAudit2    = Task("Benchmark supplier pricing",          "Compare pricing against market rates for top 10 suppliers.");
        var tAudit3    = Task("Negotiate renegotiation targets",     "Identify and prioritise contracts for renegotiation.");

        // Lean workshop tasks
        var tLean1     = Task("Select processes for improvement",    "Use value-stream mapping to prioritise the highest-waste processes.");
        var tLean2     = Task("Run Kaizen events",                   "Conduct time-boxed improvement sprints for each selected process.");
        var tLean3     = Task("Track and report waste reduction",    "Measure and report lead-time and error-rate improvements.");

        // Cloud migration tasks
        var tCloud1    = Task("Complete cloud readiness assessment", "Assess all workloads for cloud suitability and migration effort.");
        var tCloud2    = Task("Execute pilot migration",             "Migrate one non-critical workload as a proof of concept.");
        var tCloud3    = Task("Roll out full migration programme",   "Execute the full migration backlog with change-control sign-off.");

        // Security tasks
        var tSec1      = Task("Complete information asset register", "Catalogue all data assets, owners, and sensitivity classifications.");
        var tSec2      = Task("Conduct security risk assessment",    "Identify, score, and prioritise information security risks.");
        var tSec3      = Task("Remediate critical security gaps",    "Fix all critical and high-severity findings before audit.");

        // EVP tasks
        var tEVP1      = Task("Survey employees on what they value", "Run an anonymous survey to identify the top drivers of attraction and retention.");
        var tEVP2      = Task("Draft EVP statement and pillars",     "Write a compelling EVP narrative backed by employee data.");
        var tEVP3      = Task("Embed EVP in recruitment materials",  "Update careers page, job ads, and offer letters to reflect the EVP.");

        // OKR tasks
        var tOKR1      = Task("Train managers on OKR methodology",   "Run an OKR workshop for all people managers.");
        var tOKR2      = Task("Set company-level OKRs for next quarter", "Facilitate exec team session to define 3–5 company OKRs.");
        var tOKR3      = Task("Cascade OKRs to team level",          "Each team defines their team OKRs aligned to company objectives.");
        var tOKR4      = Task("Run weekly OKR check-ins",            "Establish a lightweight weekly cadence to update key result progress.");

        // LMS tasks
        var tLMS1      = Task("Define LMS requirements",             "Gather learning content and integration requirements from the business.");
        var tLMS2      = Task("Select and procure LMS platform",     "Evaluate vendors and sign contract.");
        var tLMS3      = Task("Curate initial learning content",     "Populate LMS with mandatory and recommended courses for all roles.");

        // ── Assemble entity graph ────────────────────────────────────

        var allObjectives   = new[] { objGrow, objOps, objTalent };
        var allProcesses    = new[] { pSales, pMarketing, pProduct, pCS, pProcure, pProcess, pIT, pFinance, pRecruit, pLD, pEngage, pPerf };
        var allInitiatives  = new[] {
            iSalesCRM, iSalesBDR, iSalesPartn,
            iSEO, iPPC, iEvents,
            iProdRoadmap, iUXResearch, iShipping,
            iOnboard, iCSQBR, iNPS,
            iProcAudit, iProcSRM,
            iLean, iAutomate,
            iCloud, iSecurity,
            iBudget, iReporting,
            iEVP, iATS, iReferral,
            iLMS, iMentor,
            iPulse, iWellbeing,
            iOKR, i1on1
        };
        var allTasks = new[] {
            tCRMReq, tCRMEval, tCRMData, tCRMTrain,
            tBDRHire, tBDRPlaybook, tBDRMetrics,
            tSEOAudit, tSEOContent, tSEOLinks,
            tRoadStake, tRoadPublish,
            tUXRecruit, tUXInterviews,
            tOnboard1, tOnboard2, tOnboard3,
            tAudit1, tAudit2, tAudit3,
            tLean1, tLean2, tLean3,
            tCloud1, tCloud2, tCloud3,
            tSec1, tSec2, tSec3,
            tEVP1, tEVP2, tEVP3,
            tOKR1, tOKR2, tOKR3, tOKR4,
            tLMS1, tLMS2, tLMS3
        };

        await db.SuggestionObjectives.AddRangeAsync(allObjectives);
        await db.SuggestionProcesses.AddRangeAsync(allProcesses);
        await db.SuggestionInitiatives.AddRangeAsync(allInitiatives);
        await db.SuggestionTasks.AddRangeAsync(allTasks);

        // Objective → Process links
        var objProcLinks = new List<SuggestionObjectiveProcess>
        {
            // Growth → Sales, Marketing, Product, CS
            OP(objGrow, pSales), OP(objGrow, pMarketing), OP(objGrow, pProduct), OP(objGrow, pCS),
            // Ops → Procure, Process, IT, Finance
            OP(objOps, pProcure), OP(objOps, pProcess), OP(objOps, pIT), OP(objOps, pFinance),
            // Talent → Recruit, L&D, Engage, Perf
            OP(objTalent, pRecruit), OP(objTalent, pLD), OP(objTalent, pEngage), OP(objTalent, pPerf),
        };
        await db.SuggestionObjectiveProcesses.AddRangeAsync(objProcLinks);

        // Process → Initiative links
        var procInitLinks = new List<SuggestionProcessInitiative>
        {
            PI(pSales, iSalesCRM), PI(pSales, iSalesBDR), PI(pSales, iSalesPartn),
            PI(pMarketing, iSEO), PI(pMarketing, iPPC), PI(pMarketing, iEvents),
            PI(pProduct, iProdRoadmap), PI(pProduct, iUXResearch), PI(pProduct, iShipping),
            PI(pCS, iOnboard), PI(pCS, iCSQBR), PI(pCS, iNPS),
            PI(pProcure, iProcAudit), PI(pProcure, iProcSRM),
            PI(pProcess, iLean), PI(pProcess, iAutomate),
            PI(pIT, iCloud), PI(pIT, iSecurity),
            PI(pFinance, iBudget), PI(pFinance, iReporting),
            PI(pRecruit, iEVP), PI(pRecruit, iATS), PI(pRecruit, iReferral),
            PI(pLD, iLMS), PI(pLD, iMentor),
            PI(pEngage, iPulse), PI(pEngage, iWellbeing),
            PI(pPerf, iOKR), PI(pPerf, i1on1),
        };
        await db.SuggestionProcessInitiatives.AddRangeAsync(procInitLinks);

        // Initiative → Task links
        var initTaskLinks = new List<SuggestionInitiativeTask>
        {
            IT(iSalesCRM, tCRMReq), IT(iSalesCRM, tCRMEval), IT(iSalesCRM, tCRMData), IT(iSalesCRM, tCRMTrain),
            IT(iSalesBDR, tBDRHire), IT(iSalesBDR, tBDRPlaybook), IT(iSalesBDR, tBDRMetrics),
            IT(iSEO, tSEOAudit), IT(iSEO, tSEOContent), IT(iSEO, tSEOLinks),
            IT(iProdRoadmap, tRoadStake), IT(iProdRoadmap, tRoadPublish),
            IT(iUXResearch, tUXRecruit), IT(iUXResearch, tUXInterviews),
            IT(iOnboard, tOnboard1), IT(iOnboard, tOnboard2), IT(iOnboard, tOnboard3),
            IT(iProcAudit, tAudit1), IT(iProcAudit, tAudit2), IT(iProcAudit, tAudit3),
            IT(iLean, tLean1), IT(iLean, tLean2), IT(iLean, tLean3),
            IT(iCloud, tCloud1), IT(iCloud, tCloud2), IT(iCloud, tCloud3),
            IT(iSecurity, tSec1), IT(iSecurity, tSec2), IT(iSecurity, tSec3),
            IT(iEVP, tEVP1), IT(iEVP, tEVP2), IT(iEVP, tEVP3),
            IT(iOKR, tOKR1), IT(iOKR, tOKR2), IT(iOKR, tOKR3), IT(iOKR, tOKR4),
            IT(iLMS, tLMS1), IT(iLMS, tLMS2), IT(iLMS, tLMS3),
        };
        await db.SuggestionInitiativeTasks.AddRangeAsync(initTaskLinks);

        await db.SaveChangesAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static SuggestionObjective Obj(string title, string description) =>
        new() { Title = title, Description = description };

    private static SuggestionProcess Proc(string title, string description) =>
        new() { Title = title, Description = description };

    private static SuggestionInitiative Init(string title, string description) =>
        new() { Title = title, Description = description };

    private static SuggestionTask Task(string title, string description) =>
        new() { Title = title, Description = description };

    private static SuggestionObjectiveProcess OP(SuggestionObjective o, SuggestionProcess p) =>
        new() { SuggestionObjective = o, SuggestionProcess = p };

    private static SuggestionProcessInitiative PI(SuggestionProcess p, SuggestionInitiative i) =>
        new() { SuggestionProcess = p, SuggestionInitiative = i };

    private static SuggestionInitiativeTask IT(SuggestionInitiative i, SuggestionTask t) =>
        new() { SuggestionInitiative = i, SuggestionTask = t };
}
