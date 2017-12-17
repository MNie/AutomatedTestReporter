namespace AutomatedTestReporter.Teamcity.Provider
open FSharp.Data
open System

type Project = XmlProvider<"""
            <project id="id" name="name" parentProjectId="_Root" href="/app/rest/projects/id:id" webUrl="http://www.wp.pl">
            <buildTypes count="8">
            <buildType id="step_id" name="step name" projectName="name" projectId="id" href="href" webUrl="http://www.wp.pl"/>
            <buildType id="step_id" name="step name" projectName="name" projectId="id" href="href" webUrl="http://www.wp.pl"/>
            </buildTypes>
            </project>""">
type Builds = XmlProvider<"""<builds count="15" href="/app/rest/buildTypes/id:id/builds">
            <build id="813095" buildTypeId="id" number="1.2.3.4" status="SUCCESS" state="finished" href="/app/rest" webUrl="http://www.wp.pl"/>
            <build id="813095" buildTypeId="id" number="1.2.3.4" status="SUCCESS" state="finished" href="/app/rest" webUrl="http://www.wp.pl"/>
            </builds>
            """> //basehref + href from buildType from Project and then build
type Build = XmlProvider<"""<build id="214" buildTypeId="type" number="41.2.3.4" status="SUCCESS" state="finished" href="/app/rest/build/idik" webUrl="http://www.wp.pl">
            <testOccurrences count="22" href="/app/rest/1" passed="22"/>
            </build>
            """>
type BuildResult = XmlProvider<"""<testOccurrences count="22" href="http://www.wp.pl">
            <testOccurrence id="id1" name="test: name" status="SUCCESS" duration="3529" href="/app/rest/test1"/>
            <testOccurrence id="id2" name="test: name" status="SUCCESS" duration="1255" href="/app/rest/test2"/>
            </testOccurrences>
            """>
type TestResult = XmlProvider<"""<testOccurrence id="idik" name="test: name" status="SUCCESS" duration="3529" href="/app/rest/test1">
            <details>
            trelemorelekuku
            </details>
            <test id="1222286797291484545" name="test: name" href="/app/test/testidik"/>
            </testOccurrence>
            """>

type LatestBuild =
    {
        count: int
        passed: int
        href: string
    }

type TestOccurence =
    {
        name: string
        status: string
        href: string
    }

type Test =
    {
        name: string
        duration: int
        detail: string
        status: string
    }

module Reporter =
    let private baseHref = "http://teamcityurl:8080"
    let private url postfix = sprintf "%s%s" baseHref postfix
    let private getProject(projectName: string) =
        let postfix = sprintf "/app/rest/projects/id:%s" projectName
        let data = Project.Load(url postfix)
        data.Project.BuildTypes 
        |> Seq.filter(fun x -> x.id.contains "Automated")
        |> Seq.map(fun x -> x.href)

    let private getBuilds projects =
        Seq.map ((fun x -> Builds.Load(url x).Builds |> Seq.head) >> (fun x -> x.href)) projects

    let private getLatestBuild builds =
        Seq.map ((fun x -> Build.Load(url x).Build.TestOccurences) >> (fun x -> {count = x.count; passed = x.passed; href = x.href})) builds

    let private getResult latestBuilds =
        latestBuilds
        |> Seq.map(fun x -> BuildResult.Load(url x.href).TestOccurences |> Seq.map(fun x -> {name = x.name; status = x.status; href = x.href}) |> Seq.filter(fun x -> x.status <> "SUCCESS"))

    let private getTestResult results =
        results
        |> Seq.map(fun x -> 
            x 
            |> Seq.map(fun y -> 
                let test = TestResult.Load(url y.href)
                {name = y.name; duration = test.duration; detail = test.detail; status = y.status}
            )
        )

    let GetTestResultsFor(projectName: string): string[] =
        let projects = getProject projectName
        let builds = getBuilds projects
        let latestBuild = getLatestBuild builds
        let result = getResult latestBuild
        let testResult = getTestResult result
        testResult
