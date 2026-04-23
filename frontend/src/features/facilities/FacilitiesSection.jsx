import { CrudPanel } from '../../components'

export function FacilitiesSection({ data, openCreate, openEdit, deleteEntity, getPanelProps }) {
  return (
    <>
      <CrudPanel entityKey="buildings" rows={data.buildings} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('facilities-buildings')} />
      <CrudPanel entityKey="rooms" rows={data.rooms} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('facilities-rooms')} />
    </>
  )
}
