namespace AutomatedTestReporter.Teamcity.Provider
open FSharp.Data
open System

type Project = XmlProvider<"""
            <project id="id" name="name" parentProjectId="_Root" href="/app/rest/projects/id:id" webUrl="http://www.wp.pl">
            <buildTypes count="8">
            <buildType id="step_id" name="step name" projectName="name" projectId="id" href="href" webUrl="http://www.wp.pl"/>
            <buildType id="step_id" name="step name" projectName="name" projectId="id" href="href" webUrl="http://www.wp.pl"/>
            </buildTypes>
            </project>
        """>
type Builds = XmlProvider<"""<builds count="15" href="/app/rest/buildTypes/id:id/builds">
            <build id="813095" buildTypeId="id" number="1.2.3.4" status="SUCCESS" state="finished" href="/app/rest" webUrl="http://www.wp.pl"/>
            <build id="813095" buildTypeId="id" number="1.2.3.4" status="SUCCESS" state="finished" href="/app/rest" webUrl="http://www.wp.pl"/>
            </builds>
            """>
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
        Count: int
        Passed: int
        Href: string
    }

type TestOccurence =
    {
        Name: string
        Status: string
        Href: string
    }

type Test =
    {
        Name: string
        Duration: int
        Detail: string
        Status: string
    }

type Reporter() =
    let baseHref = "http://teamcity:8080"
    let url postfix = sprintf "%s%s" baseHref postfix
    let getProject(projectName: string) =
        let postfix = sprintf "/app/rest/projects/id:%s" projectName
        let data = Project.Load(url postfix)
        data.BuildTypes.BuildTypes 
        |> Seq.filter(fun x -> x.Id.Contains "Automated")
        |> Seq.map(fun x -> x.Href)

    let getBuilds projects =
        Seq.map ((fun x -> Builds.Load(url x).Builds |> Seq.head) >> (fun x -> x.Href)) projects

    let getLatestBuild builds =
        Seq.map ((fun x -> 
            let build = Build.Load(url x)
            build.TestOccurrences
        ) 
        >> (fun x -> {Count = x.Count; Passed = x.Passed; Href = x.Href})) builds

    let getResult (latestBuilds: seq<LatestBuild>) =
        latestBuilds
        |> Seq.map(fun x -> 
            let result = BuildResult.Load(url x.Href)
            result.TestOccurrences 
            |> Seq.map(fun x -> {Name = x.Name; Status = x.Status; Href = x.Href}) 
            |> Seq.filter(fun x -> x.Status <> "SUCCESS")
        )

    let getTestResult results =
        results
        |> Seq.map(fun x -> 
            x 
            |> Seq.map(fun y -> 
                let test = TestResult.Load(url y.Href)
                {Name = y.Name; Duration = test.Duration; Detail = test.Details; Status = y.Status}
            )
        )

    member this.GetTestResultsFor(projectName: string): seq<seq<Test>> =
        let projects = getProject projectName
        let builds = getBuilds projects
        let latestBuild = getLatestBuild builds
        let result = getResult latestBuild
        let testResult = getTestResult result
        testResult
