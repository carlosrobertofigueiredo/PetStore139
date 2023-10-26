// 1 - Bibliotecas
using Models;
using Newtonsoft.Json; // dependencia para o JsonConvert
using RestSharp;

// 2 - NameSpace
namespace Pet;

// 3 - Classe
public class PetTest
{
    // 3.1 - Atributos
    // Endereço da API
    private const string BASE_URL = "https://petstore.swagger.io/v2/";

    // public String token;  // seria uma forma de fazer

    public static IEnumerable<TestCaseData> getTestData()
    {
        String caminhoMassa = @"C:\Iterasys\PetStore139\fixtures\pets.csv";

        using var reader = new StreamReader(caminhoMassa);

        // Pula a primeira linha com os cabeçahos
        reader.ReadLine();

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            var values = line.Split(",");

            yield return new TestCaseData(int.Parse(values[0]), int.Parse(values[1]), values[2], values[3], values[4], values[5], values[6], values[7]);
        }

    }


    // 3.2 - Funções e Métodos
    [Test, Order(1)]
    public void PostPetTest()
    {
        // Configura
        // instancia o objeto do tipo RestClient com o endereço da API
        var client = new RestClient(BASE_URL);

        // instancia o objeto do tipo RestRequest com o complemento de endereço
        // como "pet" e configurando o método para ser um post (inclusão)
        var request = new RestRequest("pet", Method.Post);

        // armazena o conteúdo do arquivo pet1.json na memória
        String jsonBody = File.ReadAllText(@"C:\Iterasys\PetStore139\fixtures\pet1.json");

        // adiciona na requisição o conteúdo do arquivo pet1.json
        request.AddBody(jsonBody);

        // Executa
        // executa a requisição conforme a configuração realizada
        // guarda o json retornado no objeto response
        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        // Exibe o responseBody no console
        Console.WriteLine(responseBody);

        // Valide que na resposta, o status code é igual ao resultado esperado (200)
        Assert.That((int)response.StatusCode, Is.EqualTo(200));


        //Valida o petId
        int petId = responseBody.id;
        Assert.That(petId, Is.EqualTo(350675));

        // Valida o nome do animal na resposta
        String name = responseBody.name.ToString();
        Assert.That(name, Is.EqualTo("Thor"));
        // OU
        // Assert.That(responseBody.name.ToString(), Is.EqualTo("Athena"));

        // Valida o status do animal na resposta
        String status = responseBody.status;
        Assert.That(status, Is.EqualTo("available"));

        //Armazenar os dados obtidos para usar nos próximos testes
        Environment.SetEnvironmentVariable("petId", petId.ToString());
    }

    [Test, Order(2)]
    public void GetPetTest()
    {
        //Configura
        int petId = 350675;                  // campo de pesquisa
        String petName = "Thor";            // resultado esperado
        String categoryName = "dog";
        String tagsName = "Vacinado";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Get);

        //Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petId));
        Assert.That((String)responseBody.name, Is.EqualTo(petName));
        Assert.That((String)responseBody.category.name, Is.EqualTo(categoryName));
        Assert.That((String)responseBody.tags[0].name, Is.EqualTo(tagsName));

    }

    [Test, Order(3)]
    public void PutPetTest()
    {
        // Configura
        // Os dados de entrada vão formar o body da alteração
        // Vamos usar uma classe modelo
        PetModel petModel = new PetModel();
        petModel.id = 1350675;
        petModel.category = new Category(1, "dog");
        petModel.name = "Thor";
        petModel.photoUrls = new string[] { "" };
        petModel.tags = new Tag[]{new Tag(1, "Vacinado"),
                                  new Tag(2, "castrado")};
        petModel.status = "pending";

        // Transformar o modelo acima em um arquivo json
        String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);

        var client = new RestClient(BASE_URL);
        var request = new RestRequest("pet", Method.Put);
        request.AddBody(jsonBody);

        // Executa

        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);
        Console.WriteLine(responseBody);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.id, Is.EqualTo(petModel.id));
        Assert.That((String)responseBody.tags[1].name, Is.EqualTo(petModel.tags[1].name));
        Assert.That((String)responseBody.status, Is.EqualTo(petModel.status));

    }

    [Test, Order(4)]
    public void DeletePetTest()
    {
        // Configura
        String petId = Environment.GetEnvironmentVariable("petId");

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"pet/{petId}", Method.Delete);
        Console.WriteLine("Id Deletado: " + petId);
        // Executa
        var response = client.Execute(request);

        //Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        Assert.That((String)responseBody.message, Is.EqualTo(petId.ToString()));

    }

    [TestCaseSource("getTestData", new object[] { }), Order(5)]
    public void PostPetDDTest(int petId,
                              int categoryId,
                              String categoryName,
                              String petName,
                              String photoUrls,
                              String tagsIds,
                              String tagsName,
                              String status)
    {
        // Configura
        PetModel petModel = new PetModel();
        petModel.id = petId;
        petModel.category = new Category(categoryId, categoryName);
        petModel.name = petName;
        petModel.photoUrls = new string[] { photoUrls };

        //Código para gerar as multiplas tags que o pet pode ter
        //separando as informações
        String[] tagsIdsList = tagsIds.Split(";");
        String[] tagsNameList = tagsName.Split(";");
        List<Tag> tagList = new List<Tag>();

        for (int i = 0; i < tagsIdsList.Length; i++)
        {   //Vai pegar o Id
            int tagId = int.Parse(tagsIdsList[i]);
            //Vai pegar o name
            String tagName = tagsNameList[i];

            Tag tag = new Tag(tagId, tagName);
            tagList.Add(tag); // Vai juntar as informarções organizadas

        }
        //Instanciando o objeto tags e organizando em forma de array
        petModel.tags = tagList.ToArray();
        petModel.status = status;

        // A estrtura de dados está pronta, agora vamos serializar
        //Converte e serializa os dados transformando em um Json
        String jsonBody = JsonConvert.SerializeObject(petModel, Formatting.Indented);
        Console.WriteLine(jsonBody);

        // instancia o objeto do tipo RestClient com o endereço da API
        var client = new RestClient(BASE_URL);

        // instancia o objeto do tipo RestRequest com o complemento de endereço
        // como "pet" e configurando o método para ser um post (inclusão)
        var request = new RestRequest("pet", Method.Post);

        /* armazena o conteúdo do arquivo pet1.json na memória
        String jsonBody = File.ReadAllText(@"C:\Iterasys\PetStore139\fixtures\pet1.json"); */

        // adiciona na requisição o conteúdo do arquivo pet1.json
        request.AddBody(jsonBody);

        // Executa
        // executa a requisição conforme a configuração realizada
        // guarda o json retornado no objeto response
        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        // Exibe o responseBody no console
        Console.WriteLine(responseBody);

        // Valide que na resposta, o status code é igual ao resultado esperado (200)
        Assert.That((int)response.StatusCode, Is.EqualTo(200));


        //Valida o petId
        Assert.That((int)responseBody.id, Is.EqualTo(petId));

        // Valida o nome do animal na resposta
        //String name = responseBody.name.ToString();
        Assert.That((String)responseBody.name, Is.EqualTo(petName));


        // Valida o status do animal na resposta
        //String status = responseBody.status;
        Assert.That((String)responseBody.status, Is.EqualTo(status));

    }

    [Test, Order(6)]
    public void GetUserLoginTest()
    {
        // Configura
        String username = "joca";
        String password = "teste";

        var client = new RestClient(BASE_URL);
        var request = new RestRequest($"user/login?username={username}&password={password}", Method.Get);

        //https://petstore.swagger.io/v2/user/login?username=joca&password=teste

        // Executa
        var response = client.Execute(request);

        // Valida
        var responseBody = JsonConvert.DeserializeObject<dynamic>(response.Content);

        Assert.That((int)response.StatusCode, Is.EqualTo(200));
        Assert.That((int)responseBody.code, Is.EqualTo(200));
        String message = responseBody.message;
        String token = message.Substring(message.LastIndexOf(":") + 1);
        Console.WriteLine($"Token = {token}");

        Environment.SetEnvironmentVariable("token", token);

    }
}

