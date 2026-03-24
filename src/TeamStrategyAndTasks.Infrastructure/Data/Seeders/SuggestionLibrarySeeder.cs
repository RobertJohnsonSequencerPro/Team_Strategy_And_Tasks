using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities.Suggestions;

namespace TeamStrategyAndTasks.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds the read-only suggestion library hierarchy that guides users in building their strategy.
/// Clears and re-seeds on every startup so updated library content is always applied.
/// </summary>
public static class SuggestionLibrarySeeder
{
    // Increment this when the seed data changes to trigger a re-seed.
    private const int SeedVersion = 2;

    public static async Task SeedAsync(AppDbContext db)
    {
        // Only seed when the table is completely empty — admin-managed content takes precedence.
        if (await db.SuggestionObjectives.AnyAsync()) return;

        // ── Objectives ──────────────────────────────────────────────
        var objCOPQ    = Obj("Reduce Cost of Poor Quality",            "Eliminate defects, scrap, rework, and warranty costs to protect margin.");
        var objOEE     = Obj("Improve Operational Efficiency",         "Maximise throughput and minimise waste across all production operations.");
        var objOTD     = Obj("Accelerate On-Time Delivery Performance","Consistently meet customer delivery commitments and reduce lead times.");
        var objSCR     = Obj("Strengthen Supply Chain Resilience",     "Build a reliable, flexible, and low-risk supply chain.");
        var objCulture = Obj("Build a High-Performance Manufacturing Culture", "Attract, develop, and retain skilled manufacturing talent.");
        var objCI      = Obj("Drive Continuous Improvement",           "Embed a culture of structured problem-solving and incremental gains.");

        // ── Processes ────────────────────────────────────────────────

        // COPQ processes
        var pQMS       = Proc("Quality Management System",             "Define, document, and enforce quality standards across operations.");
        var pInspect   = Proc("Inspection & Testing",                  "Control product quality at incoming, in-process, and final stages.");
        var pScrap     = Proc("Scrap & Rework Reduction",              "Identify root causes of defects and drive them to zero.");
        var pWarranty  = Proc("Warranty & Returns Management",         "Capture, analyse, and resolve field failures to reduce warranty costs.");
        var pSQM       = Proc("Supplier Quality Management",           "Ensure incoming materials and components meet quality standards.");

        // OEE / Efficiency processes
        var pProdPlan  = Proc("Production Planning & Scheduling",      "Match production output to demand with minimal waste and overrun.");
        var pOEE       = Proc("Overall Equipment Effectiveness (OEE)", "Measure and improve machine availability, performance, and quality.");
        var pTPM       = Proc("Maintenance & Reliability (TPM)",       "Prevent unplanned downtime through proactive and predictive maintenance.");
        var pEnergy    = Proc("Energy & Utilities Management",         "Monitor and reduce energy consumption across the facility.");
        var pInventory = Proc("Inventory & Materials Management",      "Optimise stock levels to support production without excess capital tie-up.");

        // OTD processes
        var pSOP       = Proc("Sales & Operations Planning (S&OP)",    "Align demand, supply, and finance in a single integrated plan.");
        var pOrderMgmt = Proc("Order Management & Customer Service",   "Process orders accurately and communicate proactively with customers.");
        var pLogistics = Proc("Logistics & Distribution",              "Move finished goods to customers reliably and cost-effectively.");
        var pCapacity  = Proc("Capacity Planning",                     "Ensure sufficient production capacity to meet forecast demand.");

        // Supply Chain processes
        var pSourcing  = Proc("Strategic Sourcing & Procurement",      "Secure competitive supply of materials and services at the right cost.");
        var pSupplDev  = Proc("Supplier Development & Management",     "Build supplier capability to deliver quality, cost, and delivery targets.");
        var pSCRisk    = Proc("Supply Chain Risk Management",          "Identify and mitigate risks that could disrupt supply continuity.");
        var pInbound   = Proc("Inbound Logistics & Receiving",         "Control the flow of incoming materials into the facility.");

        // Culture processes
        var pTalent    = Proc("Talent Acquisition & Onboarding",       "Hire skilled manufacturing people and integrate them quickly.");
        var pLD        = Proc("Learning & Development",                "Build technical and leadership capability across the workforce.");
        var pEngage    = Proc("Employee Engagement & Retention",       "Create conditions where people are motivated and choose to stay.");
        var pPerfMgmt  = Proc("Performance Management",                "Set clear goals and hold people accountable through structured reviews.");
        var pHSE       = Proc("Health, Safety & Environment (HSE)",    "Protect people and the environment while maintaining compliance.");

        // CI processes
        var pLean      = Proc("Lean Manufacturing Programme",          "Systematically eliminate the 8 wastes from all production processes.");
        var pSixSigma  = Proc("Six Sigma & Quality Tools",             "Use data-driven DMAIC methodology to reduce variation.");
        var pInnovation= Proc("Innovation & Technology Adoption",      "Evaluate and implement new technologies to improve competitiveness.");
        var pStdWork   = Proc("Process Standardisation & Documentation","Capture and sustain best-practice methods across the facility.");

        // ── Initiatives ──────────────────────────────────────────────

        // --- QMS ---
        var iQMSSoftware  = Init("Implement or upgrade QMS software",             "Deploy a digital QMS to manage documents, CAPAs, audits, and NCRs.");
        var iISO9001      = Init("Achieve / renew ISO 9001 certification",        "Implement the ISO 9001 QMS standard and pass third-party certification.");
        var iSPC          = Init("Deploy statistical process control (SPC)",      "Apply control charts to key process parameters to detect drift early.");
        var iQualAlert    = Init("Establish quality alert & escalation process",  "Define a tiered response to quality escapes that prevents reoccurrence.");
        var iFAI          = Init("Roll out first-article inspection (FAI)",       "Verify new or changed parts against design intent before production release.");

        // --- Inspection & Testing ---
        var iAutoInspect  = Init("Implement automated inline inspection technology", "Deploy vision systems or gauging to detect defects without manual handling.");
        var iIQC          = Init("Build incoming material inspection programme",  "Define inspection plans for all incoming materials based on supplier risk.");
        var iCMM          = Init("Deploy coordinate measuring machine (CMM) programme", "Establish CMM measurement capability for dimensional verification.");
        var iEOLTest      = Init("Establish end-of-line functional test protocol","Define and implement a rigorous functional test before product ships.");

        // --- Scrap & Rework ---
        var iScrapTaskForce = Init("Launch scrap reduction task force",           "Cross-functional team to drive the top scrap contributors to zero.");
        var i8D           = Init("Implement 8D problem-solving process",          "Standardise the 8-Discipline methodology for all quality escapes.");
        var iDefectPareto = Init("Deploy defect tracking & Pareto analysis",      "Capture defect data at every stage and prioritise the vital few.");
        var iPokaYoke     = Init("Establish mistake-proofing (poka-yoke) programme", "Eliminate error opportunities through fixturing, interlocks, and sensors.");

        // --- Warranty & Returns ---
        var iWarrantyDash = Init("Build warranty analytics dashboard",            "Aggregate warranty claims data to identify top failure modes by part/product.");
        var iFieldFailure = Init("Implement structured field failure analysis",   "Define a teardown and analysis process for all returned warranty units.");
        var iWarrantyCost = Init("Launch warranty cost reduction programme",      "Set warranty cost targets and track monthly vs budget.");

        // --- Supplier Quality ---
        var iSupplScore   = Init("Deploy supplier scorecards",                    "Measure and publish supplier quality, delivery, and responsiveness KPIs.");
        var iSupplAudit   = Init("Build supplier audit programme",                "Conduct periodic on-site audits of critical suppliers.");
        var iAVL          = Init("Implement approved vendor list (AVL) process",  "Define the process for qualifying and managing approved suppliers.");
        var iSCAR         = Init("Launch supplier corrective action (SCAR) process", "Require root-cause and corrective action from suppliers for quality failures.");

        // --- Production Planning ---
        var iERPMES       = Init("Implement production planning software (ERP/MES)", "Deploy or optimise ERP/MES scheduling modules to replace manual planning.");
        var iMPS          = Init("Build master production schedule (MPS) process",  "Establish a formal weekly MPS that balances demand and capacity.");
        var iFiniteScheduling = Init("Deploy finite capacity scheduling",          "Use constraint-aware scheduling to produce realistic, achievable plans.");
        var iDailyMgmt    = Init("Establish daily production meeting cadence",     "Implement a structured daily tier meeting to review performance and remove blockers.");

        // --- OEE ---
        var iOEEMeasure   = Init("Launch OEE measurement programme",              "Install real-time OEE tracking on all key production equipment.");
        var iDowntimeTrack = Init("Deploy automated downtime tracking",            "Capture downtime reason codes automatically to eliminate manual logging.");
        var iSMED         = Init("Implement changeover reduction (SMED) programme","Use Single-Minute Exchange of Die methodology to halve changeover times.");
        var iCentreLine   = Init("Establish equipment centre-line documentation", "Define and maintain optimum equipment settings for each product run.");

        // --- TPM / Maintenance ---
        var iPM           = Init("Implement preventive maintenance scheduling",   "Define and schedule PM tasks for all equipment based on OEM guidelines and failure history.");
        var iPredMaint    = Init("Deploy predictive maintenance (vibration/thermal)", "Use condition-monitoring sensors to predict failures before they occur.");
        var iAutonomousMaint = Init("Launch autonomous maintenance programme",    "Train operators to perform routine equipment care and inspections.");
        var iSpareParts   = Init("Build spare parts inventory management",        "Define critical spare parts lists and ensure availability without overstock.");

        // --- Energy ---
        var iEnergyAudit  = Init("Conduct energy audit",                          "Baseline energy consumption by area, process, and equipment.");
        var iEnergyMonitor = Init("Deploy energy monitoring system",              "Install sub-metering to give real-time visibility of energy usage.");
        var iEnergyProjects = Init("Launch energy reduction projects",            "Identify and execute the top efficiency improvement opportunities.");
        var iISO50001     = Init("Pursue ISO 50001 energy management certification", "Implement the ISO 50001 standard to structure ongoing energy management.");

        // --- Inventory ---
        var iCycleCounting = Init("Implement cycle counting programme",           "Replace annual stock counts with high-frequency cycle counting by category.");
        var iWMS          = Init("Deploy warehouse management system (WMS)",      "Implement WMS to control stock movements, locations, and FIFO compliance.");
        var iMinMax       = Init("Establish min/max inventory controls",          "Set and maintain min/max levels for all raw materials and WIP.");
        var iSLOB         = Init("Launch slow-moving & obsolete (SLOB) stock review", "Regularly identify, action, and write off SLOB inventory.");

        // --- S&OP ---
        var iSOP_Process  = Init("Implement monthly S&OP process",               "Establish a formal consensus planning cycle from demand review through to exec sign-off.");
        var iDemandPlan   = Init("Deploy integrated demand planning tool",        "Implement statistical forecasting software to improve forecast accuracy.");
        var iCapDemandDash = Init("Build capacity vs demand visibility dashboard","Give planning teams real-time visibility of load vs capacity for every work centre.");

        // --- Order Management ---
        var iOMS          = Init("Implement order management system",             "Deploy OMS to automate order entry, AAR checks, and confirmation.");
        var iCustomerPortal = Init("Deploy customer portal for order tracking",   "Give customers self-service visibility of order status and delivery ETAs.");
        var iATP          = Init("Build available-to-promise (ATP) process",      "Enable accurate delivery commitment at point of order entry.");

        // --- Logistics ---
        var iRouteOpt     = Init("Optimise outbound shipping routes",             "Use routing software to reduce freight cost and improve delivery reliability.");
        var iTMS          = Init("Implement transport management system (TMS)",   "Deploy TMS for carrier management, booking, tracking, and cost control.");
        var iDeliveryScore = Init("Build customer delivery performance scorecard","Measure and publish on-time-in-full (OTIF) performance by customer.");

        // --- Capacity Planning ---
        var iBottleneck   = Init("Conduct bottleneck analysis",                   "Map the constraint in the production value stream and quantify its impact.");
        var iCapModel     = Init("Build capacity model for key work centres",     "Create a parametric model to rapidly simulate capacity scenarios.");
        var iHeijunka     = Init("Implement load levelling (heijunka) process",   "Smooth the production schedule to reduce peak loads and lead time variation.");

        // --- Strategic Sourcing ---
        var iSpendAnalysis = Init("Conduct spend analysis & category segmentation", "Analyse total spend by category, supplier, and site to prioritise sourcing activity.");
        var iStrategicSourcing = Init("Implement strategic sourcing for top categories", "Run competitive RFQ/RFPs for top spend categories to reduce total cost.");
        var iEProcurement = Init("Deploy e-procurement platform",                 "Automate purchase requisition to PO workflow to reduce cycle time and maverick spend.");
        var iBidding      = Init("Run competitive bidding events",                "Conduct structured bidding events for direct materials and indirect spend.");

        // --- Supplier Development ---
        var iSupplDevProg = Init("Launch supplier development programme",         "Assign development plans to underperforming suppliers with clear targets.");
        var iSiteAssess   = Init("Build supplier site assessment process",        "Define a structured assessment template and conduct visits to key suppliers.");
        var iSBR          = Init("Implement supplier business reviews (SBRs)",    "Hold quarterly SBRs with strategic suppliers to review performance and align roadmaps.");

        // --- Supply Chain Risk ---
        var iSCRiskAssess = Init("Conduct supply chain risk assessment",          "Score all critical components for supply risk on probability and impact.");
        var iDualSource   = Init("Develop dual-sourcing strategy for critical components", "Qualify a second source for all single-source critical parts.");
        var iBCP          = Init("Build supply continuity plans for critical materials", "Create documented contingency plans for supply disruption of critical items.");

        // --- Inbound Logistics ---
        var iDockSchedule = Init("Implement dock scheduling system",              "Coordinate inbound deliveries to eliminate dock congestion and delays.");
        var iSupplOTD     = Init("Build supplier on-time delivery scorecard",     "Measure and publish supplier delivery performance and hold regular reviews.");
        var iFreightConsolid = Init("Optimise inbound freight consolidation",     "Reduce inbound freight cost by consolidating shipments where possible.");

        // --- Talent Acquisition ---
        var iJobProfiles  = Init("Define manufacturing job profiles & competency framework", "Document role-specific competencies for all manufacturing positions.");
        var iApprenticeship = Init("Build apprenticeship & trades pipeline",      "Partner with local colleges and training providers to build a pipeline of skilled tradespeople.");
        var iReferral     = Init("Launch employee referral programme",            "Incentivise employees to refer qualified candidates for hard-to-fill roles.");
        var iATS          = Init("Implement applicant tracking system (ATS)",     "Deploy ATS to structure, accelerate, and track the hiring process.");

        // --- L&D ---
        var iSkillsMatrix = Init("Build skills matrix for all manufacturing roles", "Map current skills against requirements for every role to identify gaps.");
        var iLMS          = Init("Deploy learning management system (LMS)",       "Provide a self-serve platform for technical, safety, and compliance training.");
        var iTechTraining = Init("Launch technical skills training programme",    "Deliver structured training to close the priority skills gaps identified in the matrix.");
        var iSupervisorDev = Init("Develop leadership programme for supervisors", "Equip front-line supervisors with coaching, problem-solving, and team management skills.");

        // --- Engagement ---
        var iPulse        = Init("Launch employee pulse survey programme",        "Run regular short surveys to detect engagement issues early.");
        var iRecognition  = Init("Implement recognition & rewards programme",     "Formalise how great work is recognised across the facility.");
        var iStructuredOnboard = Init("Build structured onboarding programme for new starters", "Standardise the first-90-days experience to accelerate time to productivity.");
        var iSuggestionScheme = Init("Establish employee suggestion scheme",      "Create a formal channel for employees to submit and track improvement ideas.");

        // --- Performance Management ---
        var iOKR          = Init("Roll out OKR / goal-setting framework",         "Align company and team goals using Objectives & Key Results.");
        var iPerfReview   = Init("Implement structured performance review process", "Establish a consistent, fair review process with clear criteria and calibration.");
        var iCoaching     = Init("Build supervisor coaching & feedback skills",   "Train supervisors to hold effective development conversations and give regular feedback.");

        // --- HSE ---
        var iBBS          = Init("Implement behavioural safety observation programme", "Train all employees to conduct peer safety observations and share learnings.");
        var iISO45001     = Init("Achieve ISO 45001 occupational health & safety certification", "Implement the ISO 45001 OH&S standard and pass third-party audit.");
        var iNearMiss     = Init("Launch near-miss reporting & investigation process", "Build a culture of near-miss reporting and ensure every report receives a visible response.");
        var iEnvCompliance = Init("Build environmental compliance management programme", "Ensure ongoing compliance with all environmental permits and legislation.");

        // --- Lean ---
        var iVSM          = Init("Launch value-stream mapping workshops",         "Map the current-state value stream for top product families and design the future state.");
        var iKaizen        = Init("Establish kaizen event programme",             "Run regular focused improvement events targeting specific waste or constraint.");
        var i5S           = Init("Implement 5S workplace organisation programme","Sort, set-in-order, shine, standardise, and sustain all work areas.");
        var iVisualFactory = Init("Build visual management system (visual factory)", "Make production status, standards, and abnormalities visible at a glance.");

        // --- Six Sigma ---
        var iGreenBelt    = Init("Train Green Belt cohort",                       "Certify a cohort of employees in Six Sigma Green Belt methodology.");
        var iDMAIC        = Init("Launch DMAIC project pipeline",                 "Build and manage a portfolio of data-driven improvement projects.");
        var iMSA          = Init("Implement measurement system analysis (MSA) programme", "Validate the accuracy and repeatability of all key measurement systems.");

        // --- Innovation & Technology ---
        var iIndustry40   = Init("Conduct Industry 4.0 / digital readiness assessment", "Assess current digital maturity and define the roadmap to smart manufacturing.");
        var iIoTPilot     = Init("Pilot IoT sensors for process monitoring",      "Deploy connected sensors on a pilot line to capture real-time process data.");
        var iMES          = Init("Deploy manufacturing execution system (MES)",   "Implement MES to provide real-time production visibility and control.");
        var iAutomation   = Init("Launch automation & robotics feasibility study","Identify and prioritise automation opportunities by ROI and implementation risk.");

        // --- Process Standardisation ---
        var iSWI          = Init("Build standard work instruction (SWI) library","Document current best-practice methods for all key manufacturing operations.");
        var iDocControl   = Init("Implement document control system",             "Deploy a controlled document management system to manage revisions and obsolescence.");
        var iECM          = Init("Deploy engineering change management process",  "Define a formal gate process for approving and communicating engineering changes.");
        var iLPA          = Init("Establish layered process audit (LPA) programme", "Verify that standard work is being followed at multiple levels of the organisation.");

        // ── Tasks ─────────────────────────────────────────────────────

        // ISO 9001
        var tISO_Gap    = Task("Conduct ISO 9001 gap assessment",                 "Assess current state against ISO 9001 requirements and document gaps.");
        var tISO_Plan   = Task("Build ISO 9001 implementation plan",              "Create a project plan with owners and dates to close all gaps.");
        var tISO_Docs   = Task("Develop quality manual and procedures",           "Write or update quality documentation to meet standard requirements.");
        var tISO_Audit  = Task("Complete internal audit before certification",    "Run a full internal audit cycle and close all non-conformances.");
        var tISO_Cert   = Task("Engage certification body and pass Stage 1 & 2", "Select a UKAS-accredited body, complete stage 1 desk review and stage 2 site audit.");

        // SPC
        var tSPC_Train  = Task("Train engineers in SPC methodology",              "Deliver classroom and hands-on SPC training to quality and process engineers.");
        var tSPC_PFMEA  = Task("Identify control chart candidates from PFMEA",   "Use the process FMEA to select CTQ characteristics for charting.");
        var tSPC_Charts = Task("Implement control charts on pilot line",          "Deploy Xbar-R or I-MR charts on 3 pilot CTQs and establish control limits.");
        var tSPC_React  = Task("Define reaction plans for out-of-control signals","Write documented reaction plans for each control chart.");

        // Scrap Reduction Task Force
        var tScrap_Data    = Task("Pull 12-month scrap data by defect code",       "Extract and validate historical scrap data from ERP/MES.");
        var tScrap_Pareto  = Task("Build defect Pareto and select top 3 themes",   "Identify the vital few defect types representing 80% of scrap cost.");
        var tScrap_RCCA    = Task("Complete root cause analysis for top defects",  "Use 5-Why or Ishikawa to determine true root causes.");
        var tScrap_Actions = Task("Implement and verify corrective actions",       "Execute corrective actions and confirm effectiveness with 4-week follow-up data.");

        // 8D
        var t8D_Train   = Task("Train quality team on 8D methodology",            "Deliver 8D training and provide a standard template.");
        var t8D_Register = Task("Build 8D register and tracking dashboard",       "Create a live tracker to monitor open 8Ds by status and owner.");
        var t8D_Lessons = Task("Establish lessons-learned database",              "Capture closed 8D outcomes and make searchable for future problems.");

        // OEE Measurement
        var tOEE_Define  = Task("Define OEE calculation methodology",             "Agree definitions of planned time, downtime, rate, and quality for each machine.");
        var tOEE_Baseline = Task("Run 4-week OEE baseline measurement",           "Manually collect OEE data on pilot equipment to establish baseline.");
        var tOEE_Software = Task("Select and deploy OEE data collection software","Evaluate vendors and deploy automated OEE on all key assets.");
        var tOEE_Targets  = Task("Set OEE improvement targets by machine",        "Define 12-month OEE targets with operations and maintenance teams.");

        // SMED
        var tSMED_Video  = Task("Video-record current changeover",                "Capture full current-state changeover to enable analysis.");
        var tSMED_Analyse = Task("Separate internal and external activities",     "Classify all changeover steps as internal (machine stopped) or external (can be done while running).");
        var tSMED_Convert = Task("Convert internal to external activities",       "Redesign changeover to move setup activities off the critical path.");
        var tSMED_Sustain = Task("Write standardised changeover procedure",       "Document and train the improved changeover method.");

        // Preventive Maintenance
        var tPM_Asset    = Task("Create full asset register",                     "Catalogue all equipment with make, model, install date, and criticality rating.");
        var tPM_Plans    = Task("Define PM task lists from OEM manuals",          "Extract and document PM tasks, frequencies, and spare part requirements.");
        var tPM_Schedule = Task("Load PM schedules into CMMS",                   "Enter all PM plans into the computerised maintenance management system.");
        var tPM_Compliance = Task("Establish PM compliance KPI and reporting",    "Track and report PM schedule compliance weekly.");

        // S&OP
        var tSOP_Design  = Task("Design S&OP process framework and calendar",    "Define the meeting structure, inputs/outputs, and monthly review calendar.");
        var tSOP_Roles   = Task("Assign S&OP roles and responsibilities",        "Designate process owner, demand planner, supply planner, and executive sponsor.");
        var tSOP_Pilot   = Task("Run 3-month S&OP pilot",                        "Execute the S&OP cycle for one business unit before full rollout.");
        var tSOP_Tool    = Task("Implement S&OP planning tool",                  "Select and configure a planning tool to support the S&OP data flow.");

        // 5S
        var t5S_Train    = Task("Train all employees in 5S methodology",          "Run 5S awareness training for every shift across the facility.");
        var t5S_Sort     = Task("Complete Sort phase in all areas",               "Remove all items not needed in each work area.");
        var t5S_SetOrder = Task("Complete Set-in-Order phase",                   "Define and mark storage locations for all tools, materials, and equipment.");
        var t5S_Audit    = Task("Establish weekly 5S audit process",             "Implement a scored 5S audit to sustain the standards achieved.");

        // VSM
        var tVSM_Select  = Task("Select product family for mapping",             "Use a product family matrix to select the highest-volume, highest-impact family.");
        var tVSM_Current = Task("Map current-state value stream",                "Walk the shop floor and map all process steps, inventory, and information flows.");
        var tVSM_Future  = Task("Design future-state value stream",              "Identify waste and design the future-state map with pull and flow principles.");
        var tVSM_Plan    = Task("Build value-stream transformation plan",         "Identify the improvement projects needed to achieve the future state.");

        // Skills Matrix
        var tSkills_Define = Task("Define skill requirements per role",           "List the technical, safety, and quality skills required for each manufacturing role.");
        var tSkills_Assess = Task("Assess current workforce skills",             "Evaluate every employee against the skill requirements and gap-score.");
        var tSkills_Plan   = Task("Build individual development plans",          "Create a prioritised training plan for each employee based on gaps and business need.");
        var tSkills_Review = Task("Establish quarterly skills matrix review",    "Schedule regular reviews so the matrix stays current as roles and requirements evolve.");

        // Pulse Survey
        var tPulse_Design  = Task("Design pulse survey instrument",              "Define 5–10 questions covering the key engagement drivers for manufacturing teams.");
        var tPulse_Launch  = Task("Run first pulse survey and analyse results",  "Deploy survey, achieve >70% response rate, and present findings to leadership.");
        var tPulse_Actions = Task("Build and communicate action plan",           "Translate survey findings into visible actions with owners and timelines.");
        var tPulse_Cadence = Task("Establish recurring survey cadence",          "Schedule bi-monthly pulse surveys and quarterly deep-dives.");

        // OKR
        var tOKR_Train   = Task("Train managers on OKR methodology",             "Run OKR workshop for all people managers.");
        var tOKR_Company = Task("Set company-level OKRs",                        "Facilitate exec session to define 3–5 top-level OKRs for the year.");
        var tOKR_Cascade = Task("Cascade OKRs to team level",                   "Each team defines their OKRs aligned to company objectives.");
        var tOKR_Checkin = Task("Run weekly OKR check-ins",                     "Establish a lightweight weekly cadence to update key result progress.");

        // Near-miss reporting
        var tNM_Process  = Task("Design near-miss reporting process",            "Define what qualifies as a near-miss, reporting method, and SLA for response.");
        var tNM_Training = Task("Train all employees to report near-misses",     "Run toolbox talks on how and why to report near-misses.");
        var tNM_Dashboard = Task("Build near-miss tracking dashboard",           "Report frequency rate, open investigations, and closed actions weekly.");
        var tNM_Learn    = Task("Share learnings from closed investigations",    "Feed back investigation outcomes to the team to close the loop.");

        // ── Assemble entity graph ────────────────────────────────────

        var allObjectives = new[] { objCOPQ, objOEE, objOTD, objSCR, objCulture, objCI };

        var allProcesses = new[]
        {
            pQMS, pInspect, pScrap, pWarranty, pSQM,
            pProdPlan, pOEE, pTPM, pEnergy, pInventory,
            pSOP, pOrderMgmt, pLogistics, pCapacity,
            pSourcing, pSupplDev, pSCRisk, pInbound,
            pTalent, pLD, pEngage, pPerfMgmt, pHSE,
            pLean, pSixSigma, pInnovation, pStdWork
        };

        var allInitiatives = new[]
        {
            iQMSSoftware, iISO9001, iSPC, iQualAlert, iFAI,
            iAutoInspect, iIQC, iCMM, iEOLTest,
            iScrapTaskForce, i8D, iDefectPareto, iPokaYoke,
            iWarrantyDash, iFieldFailure, iWarrantyCost,
            iSupplScore, iSupplAudit, iAVL, iSCAR,
            iERPMES, iMPS, iFiniteScheduling, iDailyMgmt,
            iOEEMeasure, iDowntimeTrack, iSMED, iCentreLine,
            iPM, iPredMaint, iAutonomousMaint, iSpareParts,
            iEnergyAudit, iEnergyMonitor, iEnergyProjects, iISO50001,
            iCycleCounting, iWMS, iMinMax, iSLOB,
            iSOP_Process, iDemandPlan, iCapDemandDash,
            iOMS, iCustomerPortal, iATP,
            iRouteOpt, iTMS, iDeliveryScore,
            iBottleneck, iCapModel, iHeijunka,
            iSpendAnalysis, iStrategicSourcing, iEProcurement, iBidding,
            iSupplDevProg, iSiteAssess, iSBR,
            iSCRiskAssess, iDualSource, iBCP,
            iDockSchedule, iSupplOTD, iFreightConsolid,
            iJobProfiles, iApprenticeship, iReferral, iATS,
            iSkillsMatrix, iLMS, iTechTraining, iSupervisorDev,
            iPulse, iRecognition, iStructuredOnboard, iSuggestionScheme,
            iOKR, iPerfReview, iCoaching,
            iBBS, iISO45001, iNearMiss, iEnvCompliance,
            iVSM, iKaizen, i5S, iVisualFactory,
            iGreenBelt, iDMAIC, iMSA,
            iIndustry40, iIoTPilot, iMES, iAutomation,
            iSWI, iDocControl, iECM, iLPA
        };

        var allTasks = new[]
        {
            tISO_Gap, tISO_Plan, tISO_Docs, tISO_Audit, tISO_Cert,
            tSPC_Train, tSPC_PFMEA, tSPC_Charts, tSPC_React,
            tScrap_Data, tScrap_Pareto, tScrap_RCCA, tScrap_Actions,
            t8D_Train, t8D_Register, t8D_Lessons,
            tOEE_Define, tOEE_Baseline, tOEE_Software, tOEE_Targets,
            tSMED_Video, tSMED_Analyse, tSMED_Convert, tSMED_Sustain,
            tPM_Asset, tPM_Plans, tPM_Schedule, tPM_Compliance,
            tSOP_Design, tSOP_Roles, tSOP_Pilot, tSOP_Tool,
            t5S_Train, t5S_Sort, t5S_SetOrder, t5S_Audit,
            tVSM_Select, tVSM_Current, tVSM_Future, tVSM_Plan,
            tSkills_Define, tSkills_Assess, tSkills_Plan, tSkills_Review,
            tPulse_Design, tPulse_Launch, tPulse_Actions, tPulse_Cadence,
            tOKR_Train, tOKR_Company, tOKR_Cascade, tOKR_Checkin,
            tNM_Process, tNM_Training, tNM_Dashboard, tNM_Learn
        };

        await db.SuggestionObjectives.AddRangeAsync(allObjectives);
        await db.SuggestionProcesses.AddRangeAsync(allProcesses);
        await db.SuggestionInitiatives.AddRangeAsync(allInitiatives);
        await db.SuggestionTasks.AddRangeAsync(allTasks);

        // ── Objective → Process links ────────────────────────────────
        var objProcLinks = new List<SuggestionObjectiveProcess>
        {
            // COPQ → QMS, Inspection, Scrap, Warranty, Supplier Quality
            OP(objCOPQ, pQMS), OP(objCOPQ, pInspect), OP(objCOPQ, pScrap), OP(objCOPQ, pWarranty), OP(objCOPQ, pSQM),
            // Efficiency → Production Planning, OEE, TPM, Energy, Inventory
            OP(objOEE, pProdPlan), OP(objOEE, pOEE), OP(objOEE, pTPM), OP(objOEE, pEnergy), OP(objOEE, pInventory),
            // OTD → S&OP, Order Mgmt, Logistics, Capacity
            OP(objOTD, pSOP), OP(objOTD, pOrderMgmt), OP(objOTD, pLogistics), OP(objOTD, pCapacity),
            // Supply Chain → Sourcing, Supplier Dev, SC Risk, Inbound
            OP(objSCR, pSourcing), OP(objSCR, pSupplDev), OP(objSCR, pSCRisk), OP(objSCR, pInbound),
            // Culture → Talent, L&D, Engage, PerfMgmt, HSE
            OP(objCulture, pTalent), OP(objCulture, pLD), OP(objCulture, pEngage), OP(objCulture, pPerfMgmt), OP(objCulture, pHSE),
            // CI → Lean, Six Sigma, Innovation, Standardisation
            OP(objCI, pLean), OP(objCI, pSixSigma), OP(objCI, pInnovation), OP(objCI, pStdWork),
        };
        await db.SuggestionObjectiveProcesses.AddRangeAsync(objProcLinks);

        // ── Process → Initiative links ───────────────────────────────
        var procInitLinks = new List<SuggestionProcessInitiative>
        {
            PI(pQMS,      iQMSSoftware), PI(pQMS,      iISO9001),   PI(pQMS,      iSPC),         PI(pQMS,      iQualAlert),  PI(pQMS,      iFAI),
            PI(pInspect,  iAutoInspect), PI(pInspect,  iIQC),        PI(pInspect,  iCMM),         PI(pInspect,  iEOLTest),
            PI(pScrap,    iScrapTaskForce), PI(pScrap,  i8D),         PI(pScrap,    iDefectPareto), PI(pScrap,    iPokaYoke),
            PI(pWarranty, iWarrantyDash), PI(pWarranty, iFieldFailure), PI(pWarranty, iWarrantyCost),
            PI(pSQM,      iSupplScore),  PI(pSQM,      iSupplAudit), PI(pSQM,      iAVL),         PI(pSQM,      iSCAR),
            PI(pProdPlan, iERPMES),      PI(pProdPlan, iMPS),        PI(pProdPlan, iFiniteScheduling), PI(pProdPlan, iDailyMgmt),
            PI(pOEE,      iOEEMeasure),  PI(pOEE,      iDowntimeTrack), PI(pOEE,   iSMED),        PI(pOEE,      iCentreLine),
            PI(pTPM,      iPM),          PI(pTPM,      iPredMaint),  PI(pTPM,      iAutonomousMaint), PI(pTPM,    iSpareParts),
            PI(pEnergy,   iEnergyAudit), PI(pEnergy,   iEnergyMonitor), PI(pEnergy, iEnergyProjects), PI(pEnergy, iISO50001),
            PI(pInventory, iCycleCounting), PI(pInventory, iWMS),    PI(pInventory, iMinMax),      PI(pInventory, iSLOB),
            PI(pSOP,      iSOP_Process), PI(pSOP,      iDemandPlan), PI(pSOP,      iCapDemandDash),
            PI(pOrderMgmt, iOMS),        PI(pOrderMgmt, iCustomerPortal), PI(pOrderMgmt, iATP),
            PI(pLogistics, iRouteOpt),   PI(pLogistics, iTMS),       PI(pLogistics, iDeliveryScore),
            PI(pCapacity,  iBottleneck), PI(pCapacity,  iCapModel),  PI(pCapacity,  iHeijunka),
            PI(pSourcing,  iSpendAnalysis), PI(pSourcing, iStrategicSourcing), PI(pSourcing, iEProcurement), PI(pSourcing, iBidding),
            PI(pSupplDev,  iSupplDevProg), PI(pSupplDev, iSiteAssess), PI(pSupplDev, iSBR),
            PI(pSCRisk,    iSCRiskAssess), PI(pSCRisk,  iDualSource), PI(pSCRisk,  iBCP),
            PI(pInbound,   iDockSchedule), PI(pInbound, iSupplOTD),  PI(pInbound,  iFreightConsolid),
            PI(pTalent,    iJobProfiles), PI(pTalent,   iApprenticeship), PI(pTalent, iReferral),  PI(pTalent, iATS),
            PI(pLD,        iSkillsMatrix), PI(pLD,      iLMS),        PI(pLD,       iTechTraining), PI(pLD,     iSupervisorDev),
            PI(pEngage,    iPulse),       PI(pEngage,   iRecognition), PI(pEngage,  iStructuredOnboard), PI(pEngage, iSuggestionScheme),
            PI(pPerfMgmt,  iOKR),         PI(pPerfMgmt, iPerfReview), PI(pPerfMgmt, iCoaching),
            PI(pHSE,       iBBS),         PI(pHSE,      iISO45001),   PI(pHSE,      iNearMiss),    PI(pHSE,    iEnvCompliance),
            PI(pLean,      iVSM),         PI(pLean,     iKaizen),     PI(pLean,     i5S),           PI(pLean,   iVisualFactory),
            PI(pSixSigma,  iGreenBelt),   PI(pSixSigma, iDMAIC),      PI(pSixSigma, iMSA),
            PI(pInnovation, iIndustry40), PI(pInnovation, iIoTPilot), PI(pInnovation, iMES),       PI(pInnovation, iAutomation),
            PI(pStdWork,   iSWI),         PI(pStdWork,  iDocControl), PI(pStdWork,  iECM),          PI(pStdWork, iLPA),
        };
        await db.SuggestionProcessInitiatives.AddRangeAsync(procInitLinks);

        // ── Initiative → Task links ───────────────────────────────────
        var initTaskLinks = new List<SuggestionInitiativeTask>
        {
            IT(iISO9001,        tISO_Gap),      IT(iISO9001,        tISO_Plan),    IT(iISO9001,        tISO_Docs),    IT(iISO9001,        tISO_Audit),   IT(iISO9001,        tISO_Cert),
            IT(iSPC,            tSPC_Train),    IT(iSPC,            tSPC_PFMEA),   IT(iSPC,            tSPC_Charts),  IT(iSPC,            tSPC_React),
            IT(iScrapTaskForce, tScrap_Data),   IT(iScrapTaskForce, tScrap_Pareto), IT(iScrapTaskForce, tScrap_RCCA), IT(iScrapTaskForce, tScrap_Actions),
            IT(i8D,             t8D_Train),     IT(i8D,             t8D_Register), IT(i8D,             t8D_Lessons),
            IT(iOEEMeasure,     tOEE_Define),   IT(iOEEMeasure,     tOEE_Baseline), IT(iOEEMeasure,    tOEE_Software), IT(iOEEMeasure,    tOEE_Targets),
            IT(iSMED,           tSMED_Video),   IT(iSMED,           tSMED_Analyse), IT(iSMED,          tSMED_Convert), IT(iSMED,          tSMED_Sustain),
            IT(iPM,             tPM_Asset),     IT(iPM,             tPM_Plans),    IT(iPM,             tPM_Schedule), IT(iPM,             tPM_Compliance),
            IT(iSOP_Process,    tSOP_Design),   IT(iSOP_Process,    tSOP_Roles),   IT(iSOP_Process,    tSOP_Pilot),   IT(iSOP_Process,    tSOP_Tool),
            IT(i5S,             t5S_Train),     IT(i5S,             t5S_Sort),     IT(i5S,             t5S_SetOrder), IT(i5S,             t5S_Audit),
            IT(iVSM,            tVSM_Select),   IT(iVSM,            tVSM_Current), IT(iVSM,            tVSM_Future),  IT(iVSM,            tVSM_Plan),
            IT(iSkillsMatrix,   tSkills_Define), IT(iSkillsMatrix,  tSkills_Assess), IT(iSkillsMatrix,  tSkills_Plan), IT(iSkillsMatrix,   tSkills_Review),
            IT(iPulse,          tPulse_Design), IT(iPulse,          tPulse_Launch), IT(iPulse,         tPulse_Actions), IT(iPulse,         tPulse_Cadence),
            IT(iOKR,            tOKR_Train),    IT(iOKR,            tOKR_Company), IT(iOKR,            tOKR_Cascade), IT(iOKR,            tOKR_Checkin),
            IT(iNearMiss,       tNM_Process),   IT(iNearMiss,       tNM_Training), IT(iNearMiss,       tNM_Dashboard), IT(iNearMiss,      tNM_Learn),
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
