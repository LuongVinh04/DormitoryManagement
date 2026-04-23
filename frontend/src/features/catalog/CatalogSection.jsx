import { CrudPanel } from '../../components'

export function CatalogSection({ data, openCreate, openEdit, deleteEntity, getPanelProps }) {
  return (
    <>
      <CrudPanel entityKey="roomCategories" rows={data.roomCategories} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('catalog-room-categories')} />
      <CrudPanel entityKey="roomZones" rows={data.roomZones} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('catalog-room-zones')} />
      <CrudPanel entityKey="paymentMethods" rows={data.paymentMethods} onCreate={openCreate} onEdit={openEdit} onDelete={deleteEntity} panelProps={getPanelProps('catalog-payment-methods')} />
    </>
  )
}
