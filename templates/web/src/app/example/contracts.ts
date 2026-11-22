export type ExampleCollectionDto = {
  id: string;
  created: string;
  name: string;
};

export type ExampleValueObjectDto = {
  code: string;
  value: number;
};

export type ExampleDetailsDto = {
  id: string;
  name: string;
  description: string;
  exampleValueObject: ExampleValueObjectDto;
  remoteCode: number | null;
};

export type ExampleCreatedEvent = {
  correlationId: string;
  id: string;
};

export type ExampleUpdatedEvent = {
  correlationId: string;
  id: string;
};

export type ExampleRemoteCodeSetEvent = {
  correlationId: string;
  id: string;
};
